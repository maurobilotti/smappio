using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smappio_SEAR
{
    public class UIControls
    {
        public UIControls(ref WaveformPainter wavePainter, ref VolumeMeter volumeMeter)
        {
            this.WavePainter = wavePainter;
            this.VolumeMeter = volumeMeter;
        }
        public WaveformPainter WavePainter;
        public VolumeMeter VolumeMeter;        
    }
}
