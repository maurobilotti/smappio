namespace Smappio_SEAR
{
    public class Log
    {
        public Log()
        {
        }

        public Log(int id, LogAudit audit)
        {
            this.Id = id;
            this.LogAudit = audit;
        }

        public int Id { get; set; }
        public LogAudit LogAudit { get; set; }

    }

    public enum LogAudit
    {
        ON,
        OFF,
        CONNECTED,
        DISCONNECTED,
        TRANSMITTED,
        TESTED
    };
}
