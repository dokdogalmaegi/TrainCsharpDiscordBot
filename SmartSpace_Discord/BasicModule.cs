using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SmartSpace_Discord // java - package
{
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        static IVoiceChannel _channel;
        EmbedBuilder builder;

        [Command("join", RunMode = RunMode.Async), Alias("����")] // message�� !join�� ������ �Ʒ� �ִ� �Լ��� ������Ѷ�
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }
            // Get the audio channel
            _channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            
            await _channel.ConnectAsync();
        }
        [Command("leave", RunMode = RunMode.Async), Alias("����")] // message�� !leave�� ������ �Ʒ� �ִ� �Լ��� ������Ѷ�
        public async Task LeaveChannel()
        {
            if (_channel == null) { await Context.Channel.SendMessageAsync("���� ���� �Ҿ����ϴ�."); return; }
            else await _channel.DisconnectAsync();
        }
        [Command("dice", RunMode = RunMode.Async), Alias("�ֻ���")]
        public async Task Dice()
        {
            Random rand = new Random();
            string dice = string.Format("�ֻ����� ��� : {0}", rand.Next(1, 6)); // �������� 1 ~ 6�� ���� �ް� ����ϱ�
            await Context.Channel.SendMessageAsync(dice);
        }
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
        [Command("help", RunMode = RunMode.Async), Alias("������")]
        public async Task Help()
        {
            builder = new EmbedBuilder();
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/558305011256393739/717986439261978686/unknown.png"); // embed ����ϻ��� ����
            builder.AddField(name: "��ɾ�", value: "!help - ���� �ִ� ��ɾ�� �⺻������ ��� �����ݴϴ�.\n!�ֻ���(dice) - 6���� ���ڸ� �������� ������ݴϴ�.\n!����(join) {ChannelName} - ChannelName�� ä�η� ���ϴ�.\n!����(leave) - ���� ä���� �����ϴ�."); // embed ����
            builder.WithTimestamp(DateTime.Now); // DateTime.Now�� ���� �ð��� �޾ƿ� WithTimesteamp�� embed�� ǥ����
            builder.WithColor(Color.Blue); // embed �� ��(bar)�� ������ Blue�� �ٲ��ִ� ������ �մϴ�.

            await Context.Channel.SendMessageAsync("", false, builder.Build()); // Help�� ���� ���
        }
    } 
}