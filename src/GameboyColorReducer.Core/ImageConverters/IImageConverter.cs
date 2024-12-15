using GameboyColorReducer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core.ImageConverters
{
    internal interface IImageConverter
    {
        public WorkingImage ToWorkingImage(byte[] inputImage);

        public byte[] ToByteArray(WorkingImage workingImage);
    }
}
