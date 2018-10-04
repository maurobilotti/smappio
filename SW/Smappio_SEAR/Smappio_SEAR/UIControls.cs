using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smappio_SEAR
{
    public partial class UIParams
    {
        public UIParams(ref WaveformPainter wavePainter, ref VolumeMeter volumeMeter, PCMAudioFormat format = PCMAudioFormat.PCM_24)
        {
            this.WavePainter = wavePainter;
            this.VolumeMeter = volumeMeter;
            this.Format = format;
        }
        public WaveformPainter WavePainter;
        public VolumeMeter VolumeMeter;
        public PCMAudioFormat Format;
    }

    public enum PCMAudioFormat
    {
        PCM_24 = 0,
        PCM_32_Float = 1,
        PCM_16 = 2
    }
}
