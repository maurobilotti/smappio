using NAudio.Gui;

namespace Smappio_SEAR
{
    public partial class UIParams
    {
        public UIParams(ref WaveformPainter wavePainter, ref VolumeMeter volumeMeter, PCMAudioFormat format = PCMAudioFormat.PCM_24, Mode mode = Mode.Auscultate)
        {
            this.WavePainter = wavePainter;
            this.VolumeMeter = volumeMeter;
            this.Format = format;
            this.Mode = mode;
        }
        public WaveformPainter WavePainter;
        public VolumeMeter VolumeMeter;
        public PCMAudioFormat Format;
        public Mode Mode;
    }

    public enum PCMAudioFormat
    {
        PCM_24 = 0,
        PCM_32_Float = 1,
        PCM_16 = 2
    }

    public enum Mode
    {
        Auscultate = 0,
        Test = 1,
        Logs = 2
    }
}
