using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Discord.Audio;
using System.Diagnostics;
using Discord.Commands;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SmartSpace_Discord
{
    class Program
    {
        private DiscordSocketClient client; // 전역 함수 client 변수 할당 Ex) Java - Scanner sc = new Scanner(System.in);
        private CommandService command;
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult(); // MainAsync을 동기로 실행하지만 실제로는 비동기를 실행합니다. - MainAsync문 실행 -> GetAwaiter값을 받아올때까지 대기 -> GetResult 값을 받아오면 반환 <- 이 과정을 비동기로 처리
        }

        public Program()
        {
            client = new DiscordSocketClient();

            client.Log += Log; // client.Log에서 받아온 값을 Log함수에 넘겨 처리한후 client.Log에 귀속합니다.
            client.Ready += Ready; // 준비 되었을 떄 실행되는 이벤트
        }

        public async Task MainAsync()
        {
            await client.LoginAsync(TokenType.Bot, "NzA3NzI5NzA5MjM2MjI0MDgz.Xtj6jA.7ZDEtDPbE7YJtFWI_zyJhydnif4"); // 봇 로그인
            await client.StartAsync(); // 봇을 시작합니다.

            command = new CommandService();
            command.Log += Log;
            client.MessageReceived += MessageReceivedAsync; // MessageReceived 이벤트 핸들러에 MessageReceivedAsunc에서 받아온 실행문을 쌓아 이벤트를 실행한다.
            await command.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

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

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
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
            var msg = message as SocketUserMessage;
            if (msg == null) return;
            int argPos = 0;

            if (!(msg.HasCharPrefix('!', ref argPos) ||
                msg.HasMentionPrefix(client.CurrentUser, ref argPos)) ||
                msg.Author.IsBot)
                return;

            var context = new SocketCommandContext(client, msg);

            var result = await command.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }
    }
}
