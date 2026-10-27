// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DashCanvas {
public class Background {
    public string Id;
    public List<string> Files = new List<string>();
    public override string ToString() { return "Background " + Id; }
}
public class FitOptions {
    public int Mode; // crop, contain, stretch
    public float X = .5f, Y = .5f;
    public float Brightness = 1;
    public bool Gray;
    public bool Transparent;
}
public static class Pictures {
    public static Bitmap Load(string path) {
        using (var stream = File.OpenRead(path)) return Load(stream);
    }
    public static Bitmap Load(Stream stream) {
        using (var im = Image.FromStream(stream, true, true)) {
            if ((long)im.Width * im.Height > 80000000) throw new Exception("Please use an image smaller than 80 megapixels.");
            if (im.PropertyIdList.Contains(0x112)) {
                int o = im.GetPropertyItem(0x112).Value[0];
                RotateFlipType[] rotations = { RotateFlipType.RotateNoneFlipNone, RotateFlipType.RotateNoneFlipNone, RotateFlipType.RotateNoneFlipX, RotateFlipType.Rotate180FlipNone, RotateFlipType.Rotate180FlipX, RotateFlipType.Rotate90FlipX, RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate270FlipX, RotateFlipType.Rotate270FlipNone };
                if (o >= 1 && o <= 8) im.RotateFlip(rotations[o]);
            }
            var bitmap=im as Bitmap;
            if(bitmap==null)return new Bitmap(im);
            var decoded=new Bitmap(bitmap.Width,bitmap.Height,PixelFormat.Format32bppArgb);
            CopyExact(bitmap,decoded,0,0);
            return decoded;
        }
    }
    public static PointF Analyze(Bitmap image) {
        using (var small = new Bitmap(image, 96, 96)) {
            double sx=0, sy=0, total=0;
            for (int y=1; y<95; y++) for (int x=1; x<95; x++) {
                Color a=small.GetPixel(x,y), b=small.GetPixel(x+1,y), c=small.GetPixel(x,y+1);
                double w = Math.Abs(a.R-b.R)+Math.Abs(a.G-b.G)+Math.Abs(a.B-b.B)+Math.Abs(a.R-c.R)+Math.Abs(a.G-c.G)+Math.Abs(a.B-c.B)+8;
                w *= .5 + .5*(1-Math.Abs(x-47.5)/48)*(1-Math.Abs(y-47.5)/48);
                sx += x*w; sy += y*w; total+=w;
            }
            return new PointF((float)(sx/total/95), (float)(sy/total/95));
        }
    }
    public static RectangleF Crop(Size source, Size target, FitOptions options) {
        float sw=source.Width, sh=source.Height;
        float ratio=(float)target.Width/target.Height;
        if (sw/sh>ratio) sw=sh*ratio; else sh=sw/ratio;
        float x=Math.Max(0,Math.Min(source.Width-sw,options.X*source.Width-sw/2));
        float y=Math.Max(0,Math.Min(source.Height-sh,options.Y*source.Height-sh/2));
        return new RectangleF(x,y,sw,sh);
    }
    public static Bitmap Render(Bitmap source, Size target, FitOptions options) {
        var result=new Bitmap(target.Width,target.Height,PixelFormat.Format32bppArgb);
        using (var g=Graphics.FromImage(result)) using (var attrs=new ImageAttributes()) {
            g.Clear(options.Transparent ? Color.Transparent : Color.Black);
            g.InterpolationMode=InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode=PixelOffsetMode.HighQuality;
            g.CompositingQuality=CompositingQuality.HighQuality;
            attrs.SetWrapMode(WrapMode.TileFlipXY);
            float v=options.Brightness;
            float[][] matrix = options.Gray ? new float[][] {new float[]{.299f*v,.299f*v,.299f*v,0,0},new float[]{.587f*v,.587f*v,.587f*v,0,0},new float[]{.114f*v,.114f*v,.114f*v,0,0},new float[]{0,0,0,1,0},new float[]{0,0,0,0,1}} : new float[][] {new float[]{v,0,0,0,0},new float[]{0,v,0,0,0},new float[]{0,0,v,0,0},new float[]{0,0,0,1,0},new float[]{0,0,0,0,1}};
            attrs.SetColorMatrix(new ColorMatrix(matrix));
            RectangleF src=new RectangleF(0,0,source.Width,source.Height);
            Rectangle dst=new Rectangle(Point.Empty,target);
            if (options.Mode==0) src=Crop(source.Size,target,options);
            if (options.Mode==1) {
                float scale=Math.Min((float)target.Width/source.Width,(float)target.Height/source.Height);
                int w=Math.Max(1,(int)Math.Round(source.Width*scale)), h=Math.Max(1,(int)Math.Round(source.Height*scale));
                dst=new Rectangle((target.Width-w)/2,(target.Height-h)/2,w,h);
            }
            g.DrawImage(source,dst,src.X,src.Y,src.Width,src.Height,GraphicsUnit.Pixel,attrs);
        }
        return result;
    }
    public static byte[] Png(Bitmap bitmap) { using (var s=new MemoryStream()) { bitmap.Save(s,ImageFormat.Png); return s.ToArray(); } }
    public static void CopyExact(Bitmap source,Bitmap destination,int x,int y) {
        var a=source.LockBits(new Rectangle(0,0,source.Width,source.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
        var b=destination.LockBits(new Rectangle(x,y,source.Width,source.Height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
        try {var row=new byte[source.Width*4];for(int i=0;i<source.Height;i++){Marshal.Copy(IntPtr.Add(a.Scan0,i*a.Stride),row,0,row.Length);Marshal.Copy(row,0,IntPtr.Add(b.Scan0,i*b.Stride),row.Length);}}
        finally {source.UnlockBits(a);destination.UnlockBits(b);}
    }
}
public class Transaction {
    public static void Atomic(string path, byte[] bytes) {
        if (bytes==null) { if(File.Exists(path)) File.Delete(path); return; }
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temp=Path.Combine(Path.GetDirectoryName(path),".dc-"+Guid.NewGuid().ToString("N")+".tmp");
        try { File.WriteAllBytes(temp,bytes); if(File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path); }
        finally { if(File.Exists(temp)) File.Delete(temp); }
    }
    public static void Commit(Dictionary<string,byte[]> writes, string backupRoot, Action<int> fault=null) {
        string backup=Path.Combine(backupRoot,DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,8));
        Directory.CreateDirectory(backup);
        var previous=new Dictionary<string,byte[]>();
        var journal=new List<object>(); int index=0;
        foreach(var w in writes) {
            byte[] old=File.Exists(w.Key)?File.ReadAllBytes(w.Key):null;
            previous.Add(w.Key,old);
            string name=(index++).ToString("D3")+".bin";
            if(old!=null) File.WriteAllBytes(Path.Combine(backup,name),old);
            journal.Add(new {path=w.Key,existed=old!=null,backupFile=name});
        }
        File.WriteAllText(Path.Combine(backup,"journal.json"),new JavaScriptSerializer().Serialize(journal));
        var changed=new List<string>();
        try {
            index=0;
            foreach(var w in writes) { if(fault!=null) fault(index); Atomic(w.Key,w.Value); changed.Add(w.Key); index++; }
            File.WriteAllText(Path.Combine(backup,"result.txt"),"Completed");
        } catch(Exception error) {
            var failures=new List<string>();
            changed.Reverse();
            foreach(var path in changed) try { Atomic(path,previous[path]); } catch(Exception e) { failures.Add(path+": "+e.Message); }
            File.WriteAllText(Path.Combine(backup,"result.txt"),"Failed: "+error.Message+"\r\n"+String.Join("\r\n",failures));
            throw new Exception(error.Message+ (failures.Count==0 ? "\nChanges were rolled back." : "\nSome files could not be restored. Recovery copies: "+backup+"\n"+String.Join("\n",failures)),error);
        }
    }
}
public partial class Game {
    public string Root, Resources, Saved, Pack, Backups;
    public const string PackId="local.dashcanvas-backgrounds";
    public List<Background> Backgrounds;
    static JavaScriptSerializer Json=new JavaScriptSerializer();
    public Game(string root,string savedOverride=null,string backupOverride=null) {
        Root=Path.GetFullPath(root); Resources=Path.Combine(Root,"Resources");
        if(!File.Exists(Path.Combine(Root,"GeometryDash.exe")) || !Directory.Exists(Resources)) throw new Exception("Choose the Geometry Dash folder containing GeometryDash.exe and Resources.");
        Saved=savedOverride??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),@"GeometryDash\geode\mods\geode.texture-loader\saved.json");
        Pack=Path.Combine(Root,@"geode\config\geode.texture-loader\packs\DashCanvas Backgrounds");
        Backups=backupOverride??Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"data","backups");
        var groups=new SortedDictionary<string,Background>();
        foreach(var file in Directory.GetFiles(Resources,"game_bg_*.png")) {
            var m=Regex.Match(Path.GetFileName(file),@"^game_bg_(\d+)_001(?:-hd|-uhd)?\.png$");
            if(!m.Success) continue;
            string id=m.Groups[1].Value;
            if(!groups.ContainsKey(id)) groups[id]=new Background {Id=id};
            groups[id].Files.Add(file);
        }
        Backgrounds=groups.Values.ToList();
        if(Backgrounds.Count==0) throw new Exception("No supported background PNG files were found in Resources.");
    }
    public bool HasLoader { get { return File.Exists(Path.Combine(Root,@"geode\mods\geode.texture-loader.geode")); } }
    public static string Detect() {
        var steamRoots=new List<string>();
        steamRoots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Steam"));
        using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam")) { if(key!=null && key.GetValue("SteamPath")!=null) steamRoots.Add(key.GetValue("SteamPath").ToString()); }
        var libraries=new List<string>(steamRoots);
        foreach(var steam in steamRoots) {
            var vdf=Path.Combine(steam,@"steamapps\libraryfolders.vdf");
            if(File.Exists(vdf)) foreach(Match m in Regex.Matches(File.ReadAllText(vdf),"\"path\"\\s+\"([^\"]+)\"")) libraries.Add(m.Groups[1].Value.Replace(@"\\",@"\"));
        }
        foreach(var lib in libraries.Distinct()) { var root=Path.Combine(lib,@"steamapps\common\Geometry Dash"); if(File.Exists(Path.Combine(root,"GeometryDash.exe"))) return root; }
        return null;
    }
    Dictionary<string,object> ReadSaved() {
        if(!File.Exists(Saved)) return new Dictionary<string,object>();
        var value=Json.DeserializeObject(File.ReadAllText(Saved)) as Dictionary<string,object>;
        if(value==null) throw new Exception("Texture Loader settings are not a JSON object. No settings were changed.");
        return value;
    }
    static List<object> Applied(Dictionary<string,object> data) {
        if(!data.ContainsKey("applied")) return new List<object>();
        var arr=data["applied"] as object[];
        if(arr==null) throw new Exception("Texture Loader's applied-packs format is not supported. Export a ZIP instead.");
        foreach(var item in arr) { var obj=item as Dictionary<string,object>; if(obj==null || !obj.ContainsKey("path") || !(obj["path"] is string)) throw new Exception("Texture Loader has an invalid pack entry. Export a ZIP instead."); }
        return arr.ToList();
    }
    static string EntryPath(object entry) { return ((Dictionary<string,object>)entry)["path"].ToString(); }
    static bool SamePath(string a,string b) { return String.Equals(Path.GetFullPath(a).TrimEnd('\\','/'),Path.GetFullPath(b).TrimEnd('\\','/'),StringComparison.OrdinalIgnoreCase); }
    public Bitmap Current(Background bg) {
        string file=Path.GetFileName(bg.Files.OrderBy(x=>x.Contains("-uhd")?0:x.Contains("-hd")?1:2).First());
        if(HasLoader) {
            foreach(var entry in Applied(ReadSaved())) {
                string path=EntryPath(entry);
                if(Directory.Exists(path)) {
                    foreach(var candidate in new[]{Path.Combine(path,file),Path.Combine(path,"Resources",file)}) if(File.Exists(candidate)) return Pictures.Load(candidate);
                } else if(File.Exists(path) && Path.GetExtension(path).Equals(".zip",StringComparison.OrdinalIgnoreCase)) {
                    using(var zip=ZipFile.OpenRead(path)) {
                        var found=zip.Entries.FirstOrDefault(e=>e.FullName.Equals(file,StringComparison.OrdinalIgnoreCase))??zip.Entries.FirstOrDefault(e=>e.FullName.Equals("Resources/"+file,StringComparison.OrdinalIgnoreCase));
                        if(found!=null) using(var s=found.Open()) return Pictures.Load(s);
                    }
                }
            }
        }
        return Pictures.Load(Path.Combine(Resources,file));
    }
    public Dictionary<string,byte[]> Build(Background bg,Bitmap source,FitOptions options) {
        var result=new Dictionary<string,byte[]>();
        foreach(var file in bg.Files) using(var original=Pictures.Load(file)) using(var converted=Pictures.Render(source,original.Size,options)) result.Add(Path.GetFileName(file),Pictures.Png(converted));
        return result;
    }
    public static byte[] Metadata() { return Encoding.UTF8.GetBytes(Json.Serialize(new {textureldr="1.5.0", name="DashCanvas Studio",id=PackId,version="3.0.0",author="You"})); }
    void CheckOwner() {
        if(!Directory.Exists(Pack)) return;
        var meta=Path.Combine(Pack,"pack.json");
        if(!File.Exists(meta)) {if(Directory.GetFiles(Pack,"*",SearchOption.AllDirectories).Length==0)return;throw new Exception("A folder already uses the DashCanvas pack name. Rename that folder before applying.");}
        var data=Json.DeserializeObject(File.ReadAllText(meta)) as Dictionary<string,object>;
        if(data==null || !data.ContainsKey("id") || !Object.Equals(data["id"],PackId)) throw new Exception("The existing pack folder belongs to a different pack.");
    }
    public static void RequireClosed() {
        var processes=Process.GetProcessesByName("GeometryDash");
        try { if(processes.Length>0) throw new Exception("Close Geometry Dash before applying or restoring, then try again. Your preview is ready."); }
        finally { foreach(var p in processes) p.Dispose(); }
    }
    public void Apply(Background bg,Bitmap source,FitOptions options) {
        RequireClosed();
        if(!HasLoader) throw new Exception("Install Geode Texture Loader to apply automatically, or use Export ZIP.");
        CheckOwner();
        var data=ReadSaved(); var applied=Applied(data);
        var assets=Build(bg,source,options);
        var writes=new Dictionary<string,byte[]>();
        foreach(var asset in assets) writes[Path.Combine(Pack,asset.Key)]=asset.Value;
        writes[Path.Combine(Pack,"pack.json")]=Metadata();
        applied.RemoveAll(x=>SamePath(EntryPath(x),Pack));
        applied.Insert(0,new Dictionary<string,object>{{"path",Pack}});
        data["applied"]=applied;
        writes[Saved]=Encoding.UTF8.GetBytes(Json.Serialize(data));
        RequireClosed();
        Transaction.Commit(writes,Backups);
    }
    public bool IsCustomized(Background bg) { return bg.Files.Any(x=>File.Exists(Path.Combine(Pack,Path.GetFileName(x)))); }
    public void Restore(Background bg) {
        RequireClosed(); CheckOwner();
        var data=ReadSaved(); var applied=Applied(data);
        var writes=new Dictionary<string,byte[]>();
        var names=new HashSet<string>(bg.Files.Select(Path.GetFileName),StringComparer.OrdinalIgnoreCase);
        foreach(var name in names) { var path=Path.Combine(Pack,name); if(File.Exists(path)) writes[path]=null; }
        if(writes.Count==0) throw new Exception("This background has no DashCanvas replacement to restore.");
        bool other=Directory.GetFiles(Pack,"game_bg_*.png").Any(x=>!names.Contains(Path.GetFileName(x)));
        if(!other) { applied.RemoveAll(x=>SamePath(EntryPath(x),Pack)); data["applied"]=applied; writes[Saved]=Encoding.UTF8.GetBytes(Json.Serialize(data)); }
        RequireClosed(); Transaction.Commit(writes,Backups);
    }
    public void Export(string path,Background bg,Bitmap source,FitOptions options) {
        var assets=Build(bg,source,options); assets["pack.json"]=Metadata();
        using(var buffer=new MemoryStream()) {
            using(var zip=new ZipArchive(buffer,ZipArchiveMode.Create,true)) foreach(var asset in assets) using(var stream=zip.CreateEntry(asset.Key,CompressionLevel.Optimal).Open()) stream.Write(asset.Value,0,asset.Value.Length);
            Transaction.Atomic(path,buffer.ToArray());
        }
    }
}
}
