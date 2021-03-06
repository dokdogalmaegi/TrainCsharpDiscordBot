using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord.Audio.Streams;
using System.IO;
using System.Diagnostics;
using Discord.Audio;
using VideoLibrary;
using MediaToolkit.Model;
using MediaToolkit;
using System.Threading;

namespace SmartSpace_Discord // java - package
{
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        static IVoiceChannel _channel;
        static IAudioClient audio;
        static AudioOutStream outStream;
        static CancellationTokenSource cts;
        EmbedBuilder builder;

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = @"ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string path, CancellationToken ct)
        {
            // Create FFmpeg using the previous example
            var ffmpeg = CreateStream(path);
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (Stream discord = client.CreatePCMStream(AudioApplication.Music))
            {

                try {
                    await Task.Delay(5000);
                    while (!ct.IsCancellationRequested)
                    {
                        byte[] buffer = new byte[192000];
                        int byteCount = await output.ReadAsync(buffer, 0, 192000);
                        if (byteCount <= 0) break;

                        try
                        {
                            await discord.WriteAsync(buffer, 0, byteCount);
                        }
                        catch (Exception Ex)
                        {
                            Console.WriteLine(Ex.StackTrace);
                            await discord.FlushAsync();
                            break;
                        }
                    }
                }
                finally { await discord.FlushAsync(); }
            }
        }

        // 들어가기
        [Command("join", RunMode = RunMode.Async), Alias("들어와")] // message에 !join이 들어오면 아래 있는 함수를 실행시켜라
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            if (channel == null) { await Context.Channel.SendMessageAsync("봇이 길을 잃었습니다. 어디로 들어가야 할까요?"); return; }
            // Get the audio channel
            _channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            
            audio = await _channel.ConnectAsync();
        }
        // 나가기
        [Command("leave", RunMode = RunMode.Async), Alias("나가")] // message에 !leave이 들어오면 아래 있는 함수를 실행시켜라
        public async Task LeaveChannel()
        {
            if (_channel == null) { await Context.Channel.SendMessageAsync("봇이 길을 잃었습니다."); return; }
            else await _channel.DisconnectAsync();
        }
        // 주사위
        [Command("dice", RunMode = RunMode.Async), Alias("주사위")]
        public async Task Dice(int maxNumber)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            maxNumber = maxNumber == 0 ? 6 : maxNumber;
            string dice = string.Format("주사위의 결과 : {0}", rand.Next(1, maxNumber + 1)); // 랜덤으로 1 ~ maxNumber의 값을 받고 출력하기
            await Context.Channel.SendMessageAsync(dice);
        }
        // 같은 게임을 하는사람
        [Command("friends", RunMode = RunMode.Async), Alias("친구")]
        public async Task Friends()
        {
            builder = new EmbedBuilder();
            string ActivityStr = "";
            string EqualStr = "";
            foreach (IGuildUser user in Context.Guild.Users) // 서버 내 유저들을 user에 넣으며 for문을 돌립니다.
            {
                if (user.IsBot || user.Activity == null || Context.User.Id == user.Id) continue;    // 봇, 명령어를 보낸이를 예외처리
                // 현재 활동중인 사용자 찾아 출력해주기
                if(user.Activity.Type == ActivityType.Playing) // user에 타입이 게임중(활동중) 상태일 때 if문 실행
                {
                    if (user.Nickname == null) ActivityStr += user.Username; // user.Nickname(서버 내 닉네임)이 null이 
                    else ActivityStr += user.Nickname; // 아니면 ActivityStr Username(디스코드 자체 닉네임)을 넣습니다.
                    ActivityStr += " - " + user.Activity.Name; // ActivityStr 문자열에 user.Activity.Name(플레이 하고 있는 게임 이름)
                    if (user.Activity.Details != null) ActivityStr += " - " + user.Activity.Details; // 자세한 활동을 넣습니다. Ex) Visual studio - Program.cs <- VS에서 어느 것을 하는지를 출력
                    ActivityStr += "\n"; // 개행(줄바꿈) 문자
                }
                // 명령어를 실행한 플레이어와 같은 게임 하는 사람 찾아 출력해주기
                if (Context.User.Activity != null && Context.User.Activity.Name.Equals(user.Activity.Name)) // 명령어를 실행한 플레이가 하고 있는 게임을 찾고 서버 전체의 있는 사람과 비교해보기
                {
                    if (user.Nickname == null) EqualStr += user.Username; // user.Nickname(서버 내 닉네임)이 null이면 username(디스코드 자체 닉네임)을
                    else EqualStr += user.Nickname; // user.Nickname이 존재한다면 Nickname을 string EqualStr에 넣습니다.
                    EqualStr += " - " + user.Activity.Name; // 플레이어가 하고있는 게임 이름을 string EqualStr에 넣습니다.
                    if (user.Activity.Details != null) EqualStr += " - " + user.Activity.Details; // 자세한 활동을 string EqualStr 넣습니다. Ex) Visual studio - Program.cs <- VS에서 어느 것을 하는지를 출력
                    EqualStr += "\n";
                }
            }
            builder.AddField("현재 활동중인 사용자", ActivityStr, true); // 가독성을 위해 embed로 출력
            if  (EqualStr.Equals("")) EqualStr = "친구가 없으시네요 ㅋㅋ루삥빵뽕"; // EqualStr(같은 게임을 플레이하는 사람)이 없으면 null값 return으로 error 방지
            builder.AddField("현재 같은 게임을 하는 사용자", EqualStr, true); // 같이 게임을 하는 사람을 출력
            builder.Color = Color.Green; // 옆 bar색을 조정
            await Context.Channel.SendMessageAsync("", false, builder.Build()); // 채팅에 실제 출력
            // await Context.Channel.SendMessageAsync("없습니다."); ㅎㅎ...
        }
        // help문
        [Command("help", RunMode = RunMode.Async), Alias("도와줘")]
        public async Task Help()
        {
            builder = new EmbedBuilder();
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/558305011256393739/717986439261978686/unknown.png"); // embed 썸네일사진 설정
            builder.AddField(name: "명령어", value: "!help - 현재 있는 명령어와 기본정보를 모두 보여줍니다.\n!주사위(dice) <maxNum> - <maxNum>개의 숫자를 랜덤으로 출력해줍니다.\n!들어와(join) <ChannelName> - ChannelName의 채널로 들어갑니다.\n!나가(leave) - 봇이 채널을 나갑니다.\n!팀(team) <TeamNum> - <TeamNum>만큼 팀을 나눠줍니다.\n!RPS(묵찌빠, 짱깸뽀) <가위|바위|보> - 가위바위보를 합니다."); // embed 설정
            builder.WithTimestamp(DateTime.Now); // DateTime.Now로 현재 시간을 받아와 WithTimesteamp로 embed에 표시함
            builder.WithColor(Color.Blue); // embed 옆 바(bar)의 색깔을 Blue로 바꿔주는 역할을 합니다.

            await Context.Channel.SendMessageAsync("", false, builder.Build()); // Help문 실제 출력
        }
        // 팀 나누기
        [Command("team", RunMode = RunMode.Async), Alias("팀")]
        public async Task Team(int teamCount, SocketVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel as SocketVoiceChannel;
            builder = new EmbedBuilder();
            List<SocketGuildUser> users = new List<SocketGuildUser>(channel.Users);

            Random rng = new Random((int)DateTime.Now.Ticks);
            int n = users.Count; 
            while (n > 1) // 사람 수 만큼 계속 랜덤으로 섞입니다. Ex) 4명이면 4번 섞임
            {
                n--;
                int k = rng.Next(n + 1);
                SocketGuildUser value = users[k];
                users[k] = users[n];
                users[n] = value;
            }

            if(users.Count < teamCount) // 팀의 갯수가 채널의 이용자보다 많을 경우
            {
                await Context.Channel.SendMessageAsync("팀의 갯수가 채널의 인원보다 많습니다."); 
                return; // return과 함께 메세지를 보냄
            }

            for (int i = 0; i < teamCount; i++) // 팀수만큼 for문 실행
            {
                string str = ""; // Embed에 넣을 문자열
                for (int j = 0; j < users.Count / teamCount; j++) // users(유저수) / teamCount(팀수) = 한팀에 들어갈 인원 수만큼 for문이 돌아감 Ex) 4(명) / 2(팀) = 2(명)
                {
                    if (users[i].Nickname == null) str += users[i].Username; // users[i](while문에서 섞은 인원).Nickname(서버 내 닉네임)이 null이면 username(디스코드 자체 닉네임)을
                    else str += users[i].Nickname; // users[i].Nickname이 존재한다면 Nickname을 string str에 넣습니다.
                    str += "\n"; // 줄바꿈
                }
                builder.AddField(i + 1 + "번 팀",  str, true);
            }
            await Context.Channel.SendMessageAsync("", false, builder.Build()); // 마지막 출력
        }
        // 가위바위보
        [Command("RPS", RunMode = RunMode.Async), Alias("묵찌빠, 짱깸뽀")]
        public async Task RPC(string rps)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = new Color(255, 192, 203)
            };
            string strComputerrps = ""; // 컴퓨터의 가위바위보를 담을 문자열
            int computerrps = rand.Next(1, 4); // 1, 2, 3

            switch (computerrps)
            {
                case 1:
                    strComputerrps = "가위";
                    break;
                case 2:
                    strComputerrps = "바위";
                    break;
                case 3:
                    strComputerrps = "보";
                    break;
            }
            if(rps == strComputerrps) 
            {
                builder.AddField("플레이어", rps, true);
                builder.AddField("컴퓨터", strComputerrps, true);
                builder.AddField("결과", "무승부", true);
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }else if(rps == "가위" && strComputerrps == "보" || rps == "바위" && strComputerrps == "가위" || rps == "보" && strComputerrps == "바위")
            {
                builder.AddField("플레이어", rps, true);
                builder.AddField("컴퓨터", strComputerrps, true);
                builder.AddField("결과", "승리", true);
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            else if(strComputerrps == "가위" && rps == "보" || strComputerrps == "바위" && rps == "가위" || strComputerrps == "보" && rps == "바위")
            {
                builder.AddField("플레이어", rps, true);
                builder.AddField("컴퓨터", strComputerrps, true);
                builder.AddField("결과", "패배", true);
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }
        [Command("지건", RunMode = RunMode.Async), Alias("G")]
        public async Task Gun()
        {
            await Context.User.SendMessageAsync(String.Format("{0} 지건 딱대!", Context.User.Username.ToString()));
        }
        [Command("명언", RunMode = RunMode.Async)]
        public async Task Saying()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            EmbedBuilder builder = new EmbedBuilder();

            JArray sayingArr = JArray.Parse(File.ReadAllText("saying.json")); 
            int randNum = rand.Next(sayingArr.Count);
            var saying = sayingArr[randNum].ToObject<JObject>();

            builder.AddField("말한이", saying.Value<string>("author"));
            builder.AddField("글귀", saying.Value<string>("saying"));
            builder.Color = new Color(249, 153, 106);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
        [Command("p", RunMode = RunMode.Async)]
        public async Task Plus(string Url)
        {
            try
            {
                YouTube yt = YouTube.Default;
                var video = await yt.GetVideoAsync(Url);
                if (!Directory.Exists(Environment.CurrentDirectory + "\\musics\\")) Directory.CreateDirectory(Environment.CurrentDirectory + "\\musics\\");
                
                File.WriteAllBytes(Environment.CurrentDirectory + "\\musics\\" + video.FullName.Replace(" ", ""), video.GetBytes());
                using (var engine = new Engine(@"C:\ffmpeg\bin\ffmpeg.exe"))
                {
                    var inputFile = new MediaFile { Filename = Environment.CurrentDirectory + "\\musics\\" + video.FullName.Replace(" ", "") };
                    var outputFile = new MediaFile { Filename = Environment.CurrentDirectory + "\\musics\\" + video.FullName.Replace(" ", "").Replace(video.FileExtension, ".mp3") };
                    engine.Convert(inputFile, outputFile);
                    File.Delete(Environment.CurrentDirectory + "\\musics\\" + video.FullName.Replace(" ", ""));
                    Console.WriteLine("음악 파일 다운로드 완료");
                }
                cts = new CancellationTokenSource();
                await SendAsync(audio, Environment.CurrentDirectory + "\\musics\\" + video.FullName.Replace(" ", "").Replace(video.FileExtension, ".mp3"), cts.Token);
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex.StackTrace);
            }
        }

        [Command("s", RunMode = RunMode.Async), Alias("넘겨")]
        public async Task Skip()
        {
            cts.Cancel();
        }
    } 
}