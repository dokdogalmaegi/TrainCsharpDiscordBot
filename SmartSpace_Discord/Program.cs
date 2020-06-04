using Discord;
using Discord.WebSocket;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace SmartSpace_Discord
{
    class Program
    {
        private DiscordSocketClient client; // 전역 함수 client 변수 할당 Ex) Java - Scanner sc = new Scanner(System.in);
        Random rand = new Random();
        EmbedBuilder builder = new EmbedBuilder();
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult(); // MainAsync을 동기로 실행하지만 실제로는 비동기를 실행합니다. - MainAsync문 실행 -> GetAwaiter값을 받아올때까지 대기 -> GetResult 값을 받아오면 반환 <- 이 과정을 비동기로 처리
        }

        public Program()
        {
            client = new DiscordSocketClient();

            client.Log += Log; // client.Log에서 받아온 값을 Log함수에 넘겨 처리한후 client.Log에 귀속합니다.
            client.Ready += Ready; // 준비 되었을 떄 실행되는 이벤트
            client.MessageReceived += MessageReceivedAsync; // MessageReceived 이벤트 핸들러에 MessageReceivedAsunc에서 받아온 실행문을 쌓아 이벤트를 실행한다.
        }

        public async Task MainAsync()
        {
            await client.LoginAsync(TokenType.Bot, "discordToken"); // 봇 로그인
            await client.StartAsync(); // 봇을 시작합니다.

            await Task.Delay(-1); // -1로 무한실행 - 계속 뒤에서 메인문이 돌아가게 해줍니다.
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="log">client.log에서 받아오는 매개변수</param>
        /// <returns>Log를 String으로 변환 후 Task문을 완료했다는 신호를 보냄</returns>
        private Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString()); // Log를 String으로 변환
            return Task.CompletedTask; // Log를 String으로 변환한 것을 성공적으로 끝낸후 Task가 끝났다는 것을 반환시켜줌
        }

        /// <summary>
        /// 준비 되었을 때 실행되는 함수
        /// </summary>
        /// <returns>작업의 성공여부</returns>
        private Task Ready()
        {
            Console.WriteLine($"{client.CurrentUser} 연결됨!"); // client.CurrentUser - 디스코드 봇을 으미ㅣ

            return Task.CompletedTask; // 위 print문이 성공적으로 실행되면 Task가 끝났다는 것을 반환시켜줌
        }

        private async Task MessageReceivedAsync(SocketMessage message) // SocketMessage - Type을 의미, message
        {
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/558305011256393739/717986439261978686/unknown.png"); // embed 썸네일사진 설정
            builder.AddField(name: "명령어", value: "!help - 현재 있는 명령어와 기본정보를 모두 보여줍니다.\n!주사위, !dice - 6개의 숫자를 랜덤으로 출력해줍니다." ); // embed 설정
            builder.WithTimestamp(DateTime.Now); // DateTime.Now로 현재 시간을 받아와 WithTimesteamp로 embed에 표시함
            builder.WithColor(Color.Blue);
            if (message.Author.Id == client.CurrentUser.Id) // 봇이 명령어를 쳤을 때 아무 리턴값을 주지 않는다. - 예외처리
                return;
            if (message.Content == "!주사위" || message.Content == "!dice") // 유저가 !주사위 또는 !dice라는 명령어를 디스코드에 입력하게 되면 
            {
                String returnmsg = String.Format("주사위의 결과 : {0}", rand.Next(1, 6));
                await message.Channel.SendMessageAsync(returnmsg); // Random 함수를 사용하여 랜덤값을 반환하게 한다.
            }
            if (message.Content == "!help")
                await message.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
