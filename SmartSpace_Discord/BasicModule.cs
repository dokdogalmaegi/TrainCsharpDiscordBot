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

        [Command("join", RunMode = RunMode.Async), Alias("들어와")] // message에 !join이 들어오면 아래 있는 함수를 실행시켜라
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }
            // Get the audio channel
            _channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            
            await _channel.ConnectAsync();
        }
        [Command("leave", RunMode = RunMode.Async), Alias("나가")] // message에 !leave이 들어오면 아래 있는 함수를 실행시켜라
        public async Task LeaveChannel()
        {
            if (_channel == null) { await Context.Channel.SendMessageAsync("봇이 길을 잃었습니다."); return; }
            else 
            await _channel.DisconnectAsync();
        }
        [Command("dice", RunMode = RunMode.Async), Alias("주사위")]
        public async Task Dice()
        {
            Random rand = new Random();
            String dice = String.Format("주사위의 결과 : {0}", rand.Next(1, 6));
            await Context.Channel.SendMessageAsync(dice);
        }
        [Command("help", RunMode = RunMode.Async), Alias("도와줘")]
        public async Task Help()
        {
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/558305011256393739/717986439261978686/unknown.png"); // embed 썸네일사진 설정
            builder.AddField(name: "명령어", value: "!help - 현재 있는 명령어와 기본정보를 모두 보여줍니다.\n!주사위(dice) - 6개의 숫자를 랜덤으로 출력해줍니다.\n!들어와(join) {ChannelName} - ChannelName의 채널로 들어갑니다.\n!나가(leave) - 봇이 채널을 나갑니다."); // embed 설정
            builder.WithTimestamp(DateTime.Now); // DateTime.Now로 현재 시간을 받아와 WithTimesteamp로 embed에 표시함
            builder.WithColor(Color.Blue); // embed 옆 바(bar)의 색깔을 Blue로 바꿔주는 역할을 합니다.

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    } 
}