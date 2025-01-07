using GameboyColorReducer.Core.Models;

namespace GameboyColorReducer.Core.ImageConverters
{
    internal interface IImageConverter
    {
        public WorkingImage ToWorkingImage(byte[] inputImage);

        public byte[] ToByteArray(WorkingImage workingImage);
    }
}
