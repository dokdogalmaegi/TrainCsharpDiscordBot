using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using System;
using System.Threading.Tasks;

namespace SmartSpace_Discord
{
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        static IVoiceChannel _channel;
        EmbedBuilder builder = new EmbedBuilder();

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
            else 
            await _channel.DisconnectAsync();
        }
        [Command("dice", RunMode = RunMode.Async), Alias("�ֻ���")]
        public async Task Dice()
        {
            Random rand = new Random();
            String dice = String.Format("�ֻ����� ��� : {0}", rand.Next(1, 6));
            await Context.Channel.SendMessageAsync(dice);
        }
        [Command("help", RunMode = RunMode.Async), Alias("������")]
        public async Task Help()
        {
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/558305011256393739/717986439261978686/unknown.png"); // embed ����ϻ��� ����
            builder.AddField(name: "��ɾ�", value: "!help - ���� �ִ� ��ɾ�� �⺻������ ��� �����ݴϴ�.\n!�ֻ���(dice) - 6���� ���ڸ� �������� ������ݴϴ�.\n!����(join) {ChannelName} - ChannelName�� ä�η� ���ϴ�.\n!����(leave) - ���� ä���� �����ϴ�."); // embed ����
            builder.WithTimestamp(DateTime.Now); // DateTime.Now�� ���� �ð��� �޾ƿ� WithTimesteamp�� embed�� ǥ����
            builder.WithColor(Color.Blue); // embed �� ��(bar)�� ������ Blue�� �ٲ��ִ� ������ �մϴ�.

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    } 
}