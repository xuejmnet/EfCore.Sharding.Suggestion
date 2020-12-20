using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCore.Sharding.Suggestion.Entities
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 17 December 2020 12:11:52
* @Email: 326308290@qq.com
*/
    public class LogMessageMap:IEntityTypeConfiguration<LogMessage>
    {
        public void Configure(EntityTypeBuilder<LogMessage> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Name).HasMaxLength(300);
            builder.ToTable("Log_Message");
        }
    }
}