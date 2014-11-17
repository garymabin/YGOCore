using System;
using System.IO;

namespace YGOServer
{
    //interface for classes whose instances can be written and read from stream like an parcel. 
    public interface IParcel
    {
        void ReadFromStream(Stream stream);
        void WriteToStream(Stream stream);
    }
}

