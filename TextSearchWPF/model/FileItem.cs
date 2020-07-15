namespace com.hideakin.textsearch.model
{
    class FileItem
    {
        public int Fid { get; set; }

        public string Path { get; set; }

        public int Size { get; set; }

        public int HitRows { get; set; }

        public FileItem(int fid, string path, int size, int hitRows = 0)
        {
            Fid = fid;
            Path = path;
            Size = size;
            HitRows = hitRows;
        }
    }
}
