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

namespace SmartSpace_Discord // java - package
{
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        static IVoiceChannel _channel;
        static IAudioClient audio;
        static AudioOutStream outStream; 
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

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            var ffmpeg = CreateStream(path);
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (Stream discord = client.CreatePCMStream(AudioApplication.Music))
            {
                try {
                    await Task.Delay(5000);
                    while (true)
                    {
                        byte[] buffer = new byte[153600];
                        int byteCount = await output.ReadAsync(buffer, 0, 153600);
                        if (byteCount <= 0) break;

                        try
                        {
                            await discord.WriteAsync(buffer, 0, byteCount);
                        }
                        catch (Exception Ex)
                        {
                            Console.WriteLine(Ex.StackTrace);
                        }
                    }
                }
                finally { await discord.FlushAsync(); }
            }
        }

        // ����
        [Command("join", RunMode = RunMode.Async), Alias("����")] // message�� !join�� ������ �Ʒ� �ִ� �Լ��� ������Ѷ�
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            if (channel == null) { await Context.Channel.SendMessageAsync("���� ���� �Ҿ����ϴ�. ���� ���� �ұ��?"); return; }
            // Get the audio channel
            _channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            
            audio = await _channel.ConnectAsync();
        }
        // ������
        [Command("leave", RunMode = RunMode.Async), Alias("����")] // message�� !leave�� ������ �Ʒ� �ִ� �Լ��� ������Ѷ�
        public async Task LeaveChannel()
        {
            if (_channel == null) { await Context.Channel.SendMessageAsync("���� ���� �Ҿ����ϴ�."); return; }
            else await _channel.DisconnectAsync();
        }
        // �ֻ���
        [Command("dice", RunMode = RunMode.Async), Alias("�ֻ���")]
        public async Task Dice(int maxNumber)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            maxNumber = maxNumber == 0 ? 6 : maxNumber;
            string dice = string.Format("�ֻ����� ��� : {0}", rand.Next(1, maxNumber + 1)); // �������� 1 ~ maxNumber�� ���� �ް� ����ϱ�
            await Context.Channel.SendMessageAsync(dice);
        }
        // ���� ������ �ϴ»��
        [Command("friends", RunMode = RunMode.Async), Alias("ģ��")]
        public async Task Friends()
        {
            builder = new EmbedBuilder();
            string ActivityStr = "";
            string EqualStr = "";
            foreach (IGuildUser user in Context.Guild.Users) // ���� �� �������� user�� ������ for���� �����ϴ�.
            {
                if (user.IsBot || user.Activity == null || Context.User.Id == user.Id) continue;    // ��, ��ɾ �����̸� ����ó��
                // ���� Ȱ������ ����� ã�� ������ֱ�
                if(user.Activity.Type == ActivityType.Playing) // user�� Ÿ���� ������(Ȱ����) ������ �� if�� ����
                {
                    if (user.Nickname == null) ActivityStr += user.Username; // user.Nickname(���� �� �г���)�� null�� 
                    else ActivityStr += user.Nickname; // �ƴϸ� ActivityStr Username(���ڵ� ��ü �г���)�� �ֽ��ϴ�.
                    ActivityStr += " - " + user.Activity.Name; // ActivityStr ���ڿ��� user.Activity.Name(�÷��� �ϰ� �ִ� ���� �̸�)
                    if (user.Activity.Details != null) ActivityStr += " - " + user.Activity.Details; // �ڼ��� Ȱ���� �ֽ��ϴ�. Ex) Visual studio - Program.cs <- VS���� ��� ���� �ϴ����� ���
                    ActivityStr += "\n"; // ����(�ٹٲ�) ����
                }
                // ��ɾ ������ �÷��̾�� ���� ���� �ϴ� ��� ã�� ������ֱ�
                if (Context.User.Activity != null && Context.User.Activity.Name.Equals(user.Activity.Name)) // ��ɾ ������ �÷��̰� �ϰ� �ִ� ������ ã�� ���� ��ü�� �ִ� ����� ���غ���
                {
                    if (user.Nickname == null) EqualStr += user.Username; // user.Nickname(���� �� �г���)�� null�̸� username(���ڵ� ��ü �г���)��
                    else EqualStr += user.Nickname; // user.Nickname�� �����Ѵٸ� Nickname�� string EqualStr�� �ֽ��ϴ�.
                    EqualStr += " - " + user.Activity.Name; // �÷��̾ �ϰ��ִ� ���� �̸��� string EqualStr�� �ֽ��ϴ�.
                    if (user.Activity.Details != null) EqualStr += " - " + user.Activity.Details; // �ڼ��� Ȱ���� string EqualStr �ֽ��ϴ�. Ex) Visual studio - Program.cs <- VS���� ��� ���� �ϴ����� ���
                    EqualStr += "\n";
                }
            }
            builder.AddField("���� Ȱ������ �����", ActivityStr, true); // �������� ���� embed�� ���
            if  (EqualStr.Equals("")) EqualStr = "ģ���� �����ó׿� ������滧��"; // EqualStr(���� ������ �÷����ϴ� ���)�� ������ null�� return���� error ����
            builder.AddField("���� ���� ������ �ϴ� �����", EqualStr, true); // ���� ������ �ϴ� ����� ���
            builder.Color = Color.Green; // �� bar���� ����
            await Context.Channel.SendMessageAsync("", false, builder.Build()); // ä�ÿ� ���� ���
            // await Context.Channel.SendMessageAsync("�����ϴ�."); ����...
        }
        // help��
        [Command("help", RunMode = RunMode.Async), Alias("������")]
        public async Task Help()
        {
            builder = new EmbedBuilder();
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/558305011256393739/717986439261978686/unknown.png"); // embed ����ϻ��� ����
            builder.AddField(name: "��ɾ�", value: "!help - ���� �ִ� ��ɾ�� �⺻������ ��� �����ݴϴ�.\n!�ֻ���(dice) <maxNum> - <maxNum>���� ���ڸ� �������� ������ݴϴ�.\n!����(join) <ChannelName> - ChannelName�� ä�η� ���ϴ�.\n!����(leave) - ���� ä���� �����ϴ�.\n!��(team) <TeamNum> - <TeamNum>��ŭ ���� �����ݴϴ�.\n!RPS(�����, ¯����) <����|����|��> - ������������ �մϴ�."); // embed ����
            builder.WithTimestamp(DateTime.Now); // DateTime.Now�� ���� �ð��� �޾ƿ� WithTimesteamp�� embed�� ǥ����
            builder.WithColor(Color.Blue); // embed �� ��(bar)�� ������ Blue�� �ٲ��ִ� ������ �մϴ�.

            await Context.Channel.SendMessageAsync("", false, builder.Build()); // Help�� ���� ���
        }
        // �� ������
        [Command("team", RunMode = RunMode.Async), Alias("��")]
        public async Task Team(int teamCount, SocketVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel as SocketVoiceChannel;
            builder = new EmbedBuilder();
            List<SocketGuildUser> users = new List<SocketGuildUser>(channel.Users);

            Random rng = new Random((int)DateTime.Now.Ticks);
            int n = users.Count; 
            while (n > 1) // ��� �� ��ŭ ��� �������� ���Դϴ�. Ex) 4���̸� 4�� ����
            {
                n--;
                int k = rng.Next(n + 1);
                SocketGuildUser value = users[k];
                users[k] = users[n];
                users[n] = value;
            }

            if(users.Count < teamCount) // ���� ������ ä���� �̿��ں��� ���� ���
            {
                await Context.Channel.SendMessageAsync("���� ������ ä���� �ο����� �����ϴ�."); 
                return; // return�� �Բ� �޼����� ����
            }

            for (int i = 0; i < teamCount; i++) // ������ŭ for�� ����
            {
                string str = ""; // Embed�� ���� ���ڿ�
                for (int j = 0; j < users.Count / teamCount; j++) // users(������) / teamCount(����) = ������ �� �ο� ����ŭ for���� ���ư� Ex) 4(��) / 2(��) = 2(��)
                {
                    if (users[i].Nickname == null) str += users[i].Username; // users[i](while������ ���� �ο�).Nickname(���� �� �г���)�� null�̸� username(���ڵ� ��ü �г���)��
                    else str += users[i].Nickname; // users[i].Nickname�� �����Ѵٸ� Nickname�� string str�� �ֽ��ϴ�.
                    str += "\n"; // �ٹٲ�
                }
                builder.AddField(i + 1 + "�� ��",  str, true);
            }
            await Context.Channel.SendMessageAsync("", false, builder.Build()); // ������ ���
        }
        // ����������
        [Command("RPS", RunMode = RunMode.Async), Alias("�����, ¯����")]
        public async Task RPC(string rps)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = new Color(255, 192, 203)
            };
            string strComputerrps = ""; // ��ǻ���� ������������ ���� ���ڿ�
            int computerrps = rand.Next(1, 4); // 1, 2, 3

            switch (computerrps)
            {
                case 1:
                    strComputerrps = "����";
                    break;
                case 2:
                    strComputerrps = "����";
                    break;
                case 3:
                    strComputerrps = "��";
                    break;
            }
            if(rps == strComputerrps) 
            {
                builder.AddField("�÷��̾�", rps, true);
                builder.AddField("��ǻ��", strComputerrps, true);
                builder.AddField("���", "���º�", true);
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }else if(rps == "����" && strComputerrps == "��" || rps == "����" && strComputerrps == "����" || rps == "��" && strComputerrps == "����")
            {
                builder.AddField("�÷��̾�", rps, true);
                builder.AddField("��ǻ��", strComputerrps, true);
                builder.AddField("���", "�¸�", true);
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            else if(strComputerrps == "����" && rps == "��" || strComputerrps == "����" && rps == "����" || strComputerrps == "��" && rps == "����")
            {
                builder.AddField("�÷��̾�", rps, true);
                builder.AddField("��ǻ��", strComputerrps, true);
                builder.AddField("���", "�й�", true);
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }
        [Command("����", RunMode = RunMode.Async), Alias("G")]
        public async Task Gun()
        {
            await Context.User.SendMessageAsync(String.Format("{0} ���� ����!", Context.User.Username.ToString()));
        }
        [Command("���", RunMode = RunMode.Async)]
        public async Task Saying()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            EmbedBuilder builder = new EmbedBuilder();

            JArray sayingArr = JArray.Parse(File.ReadAllText("saying.json")); 
            int randNum = rand.Next(sayingArr.Count);
            var saying = sayingArr[randNum].ToObject<JObject>();

            builder.AddField("������", saying.Value<string>("author"));
            builder.AddField("�۱�", saying.Value<string>("saying"));
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
                    File.Delete(Environment.CurrentDirectory + "\\musics\\" + video.FullName);
                }

                await SendAsync(audio, Environment.CurrentDirectory + "\\musics\\" + video.FullName.Replace(video.FileExtension, ".mp3"));
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex.StackTrace);
            }
        }
    } 
}