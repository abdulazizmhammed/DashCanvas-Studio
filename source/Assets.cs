// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;
using System.IO.Compression;

namespace DashCanvas {
public class Frame {
    public string Name;
    public int X,Y,W,H,SW,SH;
    public float OX,OY;
    public bool Rotated,AllowClip;
    public string[] Aliases=new string[0];
    public Rectangle Stored {get{return new Rectangle(X,Y,Rotated?H:W,Rotated?W:H);}}
    public Frame Copy(){return (Frame)MemberwiseClone();}
    public Bitmap Extract(Bitmap sheet) {
        if(AllowClip){var image=new Bitmap(W,H,PixelFormat.Format32bppArgb);var clipped=Rectangle.Intersect(Stored,new Rectangle(0,0,sheet.Width,sheet.Height));if(!clipped.IsEmpty)using(var g=Graphics.FromImage(image)){g.CompositingMode=CompositingMode.SourceCopy;g.DrawImage(sheet,new Rectangle(clipped.X-X,clipped.Y-Y,clipped.Width,clipped.Height),clipped,GraphicsUnit.Pixel);}return image;}
        if(W<1||H<1||X<0||Y<0||Stored.Right>sheet.Width||Stored.Bottom>sheet.Height)throw new Exception("Invalid atlas rectangle: "+Name);
        var result=sheet.Clone(Stored,PixelFormat.Format32bppArgb);
        if(Rotated) result.RotateFlip(RotateFlipType.Rotate270FlipNone);
        return result;
    }
}
public static class Plists {
    static Dictionary<string,XmlNode> Dict(XmlNode node){var d=new Dictionary<string,XmlNode>(StringComparer.Ordinal);if(node==null)return d;XmlNode key=null;foreach(XmlNode n in node.ChildNodes){if(n.NodeType!=XmlNodeType.Element)continue;if(key==null){if(n.Name!="key")throw new Exception("Invalid plist dictionary");key=n;}else{d[key.InnerText]=n;key=null;}}return d;}
    static string Text(Dictionary<string,XmlNode> d,string key,string fallback=""){return d.ContainsKey(key)?d[key].InnerText:fallback;}
    static float[] Nums(string s){return Regex.Matches(s,@"-?\d+(?:\.\d+)?").Cast<Match>().Select(m=>float.Parse(m.Value,CultureInfo.InvariantCulture)).ToArray();}
    public static List<Frame> Read(byte[] bytes) {
        var doc=new XmlDocument {XmlResolver=null};using(var ms=new MemoryStream(bytes))using(var reader=XmlReader.Create(ms,new XmlReaderSettings{DtdProcessing=DtdProcessing.Ignore,XmlResolver=null}))doc.Load(reader);
        var root=Dict(doc.DocumentElement.SelectSingleNode("dict"));if(!root.ContainsKey("frames"))return new List<Frame>();
        var result=new List<Frame>();
        foreach(var entry in Dict(root["frames"])) {
            var d=Dict(entry.Value);var f=new Frame{Name=entry.Key};
            if(d.ContainsKey("textureRect")||d.ContainsKey("frame")) {
                var r=Nums(Text(d,d.ContainsKey("textureRect")?"textureRect":"frame"));if(r.Length!=4)throw new Exception("Invalid rectangle: "+f.Name);
                f.X=(int)r[0];f.Y=(int)r[1];f.W=(int)r[2];f.H=(int)r[3];
                if(d.ContainsKey("spriteSize")){var sz=Nums(Text(d,"spriteSize"));f.W=(int)sz[0];f.H=(int)sz[1];}
                string rk=d.ContainsKey("textureRotated")?"textureRotated":"rotated";f.Rotated=d.ContainsKey(rk)&&(d[rk].Name=="true"||d[rk].InnerText=="true");
                var ss=Nums(Text(d,d.ContainsKey("spriteSourceSize")?"spriteSourceSize":"sourceSize"));f.SW=ss.Length==2?(int)ss[0]:f.W;f.SH=ss.Length==2?(int)ss[1]:f.H;
                var of=Nums(Text(d,d.ContainsKey("spriteOffset")?"spriteOffset":"offset"));if(of.Length==2){f.OX=of[0];f.OY=of[1];}
            } else if(d.ContainsKey("x")) {
                f.X=int.Parse(Text(d,"x"));f.Y=int.Parse(Text(d,"y"));f.W=int.Parse(Text(d,"width"));f.H=int.Parse(Text(d,"height"));f.SW=Math.Abs(int.Parse(Text(d,"originalWidth",f.W.ToString())));f.SH=Math.Abs(int.Parse(Text(d,"originalHeight",f.H.ToString())));f.OX=float.Parse(Text(d,"offsetX","0"),CultureInfo.InvariantCulture);f.OY=float.Parse(Text(d,"offsetY","0"),CultureInfo.InvariantCulture);
            }else throw new Exception("Unsupported sprite metadata: "+f.Name);
            if(d.ContainsKey("aliases"))f.Aliases=d["aliases"].ChildNodes.Cast<XmlNode>().Where(n=>n.Name=="string").Select(n=>n.InnerText).ToArray();
            if(f.W>0&&f.H>0) result.Add(f);
        }
        return result;
    }
    static void Value(XmlWriter w,string key,string type,string value){w.WriteElementString("key",key);w.WriteElementString(type,value);}
    public static byte[] Write(List<Frame> frames,string png,Size size) {
        using(var ms=new MemoryStream()) {
            using(var w=XmlWriter.Create(ms,new XmlWriterSettings{Encoding=new UTF8Encoding(false),Indent=true})){
                w.WriteStartDocument();w.WriteStartElement("plist");w.WriteAttributeString("version","1.0");w.WriteStartElement("dict");w.WriteElementString("key","frames");w.WriteStartElement("dict");
                foreach(var f in frames){w.WriteElementString("key",f.Name);w.WriteStartElement("dict");w.WriteElementString("key","aliases");w.WriteStartElement("array");foreach(var a in f.Aliases)w.WriteElementString("string",a);w.WriteEndElement();Value(w,"spriteOffset","string",String.Format(CultureInfo.InvariantCulture,"{{{0},{1}}}",f.OX,f.OY));Value(w,"spriteSize","string","{"+f.W+","+f.H+"}");Value(w,"spriteSourceSize","string","{"+f.SW+","+f.SH+"}");Value(w,"textureRect","string","{{"+f.X+","+f.Y+"},{"+f.W+","+f.H+"}}");w.WriteElementString("key","textureRotated");w.WriteElementString("false","");w.WriteEndElement();}
                w.WriteEndElement();w.WriteElementString("key","metadata");w.WriteStartElement("dict");Value(w,"format","integer","3");Value(w,"textureFileName","string",Path.GetFileName(png));Value(w,"realTextureFileName","string",Path.GetFileName(png));Value(w,"size","string","{"+size.Width+","+size.Height+"}");w.WriteEndElement();w.WriteEndElement();w.WriteEndElement();
            }return ms.ToArray();
        }
    }
}
public class Asset {
    public string Key,Name,Category,Group,FrameName,Kind,Symbol,Title;
    public List<string> Variants=new List<string>();
    public string SearchText {get{return (Name+" "+Title+" "+Group+" "+Category+" "+Symbol+" "+AssetNames.Aliases(Name)).ToLowerInvariant();}}
    public override string ToString(){return Title;}
}
public class AssetCatalog {
    public List<Asset> Items=new List<Asset>();
    public List<string> Issues=new List<string>();
    public Dictionary<string,List<Frame>> Frames=new Dictionary<string,List<Frame>>(StringComparer.OrdinalIgnoreCase);
    public string Resources;
    public static string GroupOf(string rel){return Regex.Replace(rel,@"-(?:uhd|hd)(?=\.[^./]+$)","",RegexOptions.IgnoreCase);}
    public static int Quality(string file){return file.Contains("-uhd.")?2:file.Contains("-hd.")?1:0;}
    public static string Relative(string root,string file){return file.Substring(root.TrimEnd('\\','/').Length+1).Replace('\\','/');}
    public AssetCatalog(string resources,Action<string> progress=null) {
        Resources=resources;var groups=new Dictionary<string,Asset>(StringComparer.Ordinal);
        var pngs=Directory.GetFiles(resources,"*.png",SearchOption.AllDirectories);
        int i=0;foreach(var file in pngs){
            string rel=Relative(resources,file),group=GroupOf(rel),plist=Path.ChangeExtension(file,".plist"),fnt=Path.ChangeExtension(file,".fnt");
            if(progress!=null && i++%150==0)progress("Scanning textures "+i+" / "+pngs.Length);
            string category=group.StartsWith("game_bg_")?"Backgrounds":group.StartsWith("ground")?"Grounds":group.StartsWith("icons/")?"Player Icons":group.IndexOf("font",StringComparison.OrdinalIgnoreCase)>=0?"Fonts & Characters":"Other Images";
            bool atlas=File.Exists(plist),font=File.Exists(fnt);
            Add(groups,"image:"+group,group,rel,null,atlas||font?"Full Sheets (Advanced)":category,"image");
            if(atlas)try {
                var frames=Plists.Read(File.ReadAllBytes(plist));Frames[rel]=frames;
                foreach(var frame in frames){string cat=Category(frame.Name,group);Add(groups,"sprite:"+group+"|"+frame.Name,group,rel,frame.Name,cat,"sprite");}
            }catch(Exception e){Issues.Add(rel+": "+e.Message);}
            if(font)try{
                var frames=FontFrames(File.ReadAllBytes(fnt));Frames[rel]=frames;
                foreach(var f in frames)Add(groups,"glyph:"+group+"|"+f.Name,group,rel,f.Name,"Fonts & Characters","glyph");
            }catch(Exception e){Issues.Add(rel+": "+e.Message);}
        }
        Items=groups.Values.OrderBy(a=>a.Title).ToList();
    }
    void Add(Dictionary<string,Asset> groups,string key,string group,string rel,string frame,string category,string kind){
        Asset a;if(!groups.TryGetValue(key,out a)){string n=frame??Path.GetFileName(group);a=new Asset{Key=key,Name=n,Group=group,FrameName=frame,Category=AssetNames.Section(n,group,category),Kind=kind,Title=AssetNames.Display(n,kind,group),Symbol=SymbolFor(n)};groups[key]=a;}a.Variants.Add(rel);
    }
    public static List<Frame> FontFrames(byte[] bytes) {
        var frames=new List<Frame>();foreach(string line in Encoding.UTF8.GetString(bytes).Split('\n')){
            if(!line.StartsWith("char "))continue;var d=Regex.Matches(line,@"(\w+)=(-?\d+)").Cast<Match>().ToDictionary(m=>m.Groups[1].Value,m=>int.Parse(m.Groups[2].Value));
            if(d["width"]<1||d["height"]<1)continue;
            frames.Add(new Frame{Name="char_"+d["id"],X=d["x"],Y=d["y"],W=d["width"],H=d["height"],SW=d["width"],SH=d["height"],AllowClip=true});
        }return frames;
    }
    public static string SymbolFor(string n){n=n.ToLowerInvariant();if(n.Contains("search"))return "Search";if(n.Contains("playbtn")||n.Contains("start"))return "Play";if(n.Contains("profile")||n.Contains("account"))return "Account";if(n.Contains("difficulty")||n.Contains("difficon")||n.Contains("logo")||n.Contains("title")||n.Contains("btn")||n.Contains("folder")||n.Contains("star"))return "Original badge";return "None";}
    static string Category(string n,string group){string s=n.ToLowerInvariant();if(s.Contains("difficulty")||s.Contains("difficon")||s.Contains("demon"))return "Difficulty Faces";if(s.Contains("logo")||s.Contains("title"))return "Logos";if(s.Contains("account")||s.Contains("profile"))return "Account & Social";if(group.StartsWith("icons/"))return "Player Icons";if(s.Contains("btn")||s.Contains("button")||s.Contains("search"))return "Buttons";if(s.Contains("ground"))return "Grounds";if(s.Contains("fire")||s.Contains("particle")||s.Contains("effect")||s.Contains("glow")||s.Contains("explod"))return "Effects";if(group.Contains("GameSheet02")||group.Contains("GameSheet01"))return "Level Objects";return "Interface & Other";}
    static string Friendly(string n,string kind){
        if(n=="GJ_playBtn_001.png")return "Play / Start";if(n=="GJ_searchBtn_001.png")return "Search";if(n=="GJ_profileButton_001.png")return "Profile / Account";if(n=="GJ_logo_001.png")return "Geometry Dash Logo";if(n=="GJ_garageBtn_001.png")return "Icon Kit / Garage";if(n=="GJ_creatorBtn_001.png")return "Level Browser / Creator";
        var m=Regex.Match(n,@"^(?:difficulty|diffIcon)_(\d+|auto)");if(m.Success){var labels=new Dictionary<string,string>{{"00","Unrated / NA"},{"01","Easy"},{"02","Normal"},{"03","Hard"},{"04","Harder"},{"05","Insane"},{"06","Hard Demon"},{"07","Easy Demon"},{"08","Medium Demon"},{"09","Insane Demon"},{"10","Extreme Demon"},{"auto","Auto"}};string t;if(labels.TryGetValue(m.Groups[1].Value,out t))return t+" · "+n;}
        return n;
    }
}
public class ResourceLayer : IDisposable {
    string directory,prefix="";ZipArchive zip;Dictionary<string,ZipArchiveEntry> entries;
    public ResourceLayer(string path){
        if(Directory.Exists(path)){directory=FindRoot(path);return;}
        if(!File.Exists(path))return;
        if(!new[]{".zip",".apk"}.Contains(Path.GetExtension(path).ToLowerInvariant()))return;
        zip=ZipFile.OpenRead(path);entries=new Dictionary<string,ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);foreach(var e in zip.Entries)entries[e.FullName.Replace('\\','/')]=e;
        if(Path.GetExtension(path).Equals(".apk",StringComparison.OrdinalIgnoreCase))prefix="assets/";
        else if(entries.ContainsKey("pack.json")||entries.ContainsKey("pack.png"))prefix="";
        else if(entries.Keys.Any(x=>x.StartsWith("Resources/",StringComparison.OrdinalIgnoreCase)))prefix="Resources/";
        else {var candidate=entries.Keys.Where(x=>x.EndsWith(".plist",StringComparison.OrdinalIgnoreCase)||Regex.IsMatch(Path.GetFileName(x),@"^(?:GJ_.*|.*-(?:hd|uhd))\.png$",RegexOptions.IgnoreCase)).OrderBy(x=>x.Count(c=>c=='/')).FirstOrDefault();if(candidate!=null)prefix=candidate.Substring(0,candidate.Length-Path.GetFileName(candidate).Length);}
    }
    static string FindRoot(string path){if(File.Exists(Path.Combine(path,"pack.json"))||File.Exists(Path.Combine(path,"pack.png")))return path;if(Directory.Exists(Path.Combine(path,"Resources")))return Path.Combine(path,"Resources");if(Directory.Exists(Path.Combine(path,"icons")))return path;if(Directory.GetFiles(path).Any(x=>Path.GetExtension(x)==".plist"||Regex.IsMatch(Path.GetFileName(x),@"^(?:GJ_.*|.*-(?:hd|uhd))\.png$")))return path;foreach(var d in Directory.GetDirectories(path)){var r=FindRoot(d);if(r!=null)return r;}return null;}
    public byte[] Read(string relative){relative=SafeRelative(relative);if(directory!=null){string file=Path.Combine(directory,relative.Replace('/','\\'));return File.Exists(file)?File.ReadAllBytes(file):null;}ZipArchiveEntry e;if(entries!=null&&entries.TryGetValue(prefix+relative,out e)){if(e.Length>300000000)throw new Exception("Texture is too large: "+relative);using(var s=e.Open())using(var ms=new MemoryStream()){s.CopyTo(ms);return ms.ToArray();}}return null;}
    public static string SafeRelative(string path){path=path.Replace('\\','/');if(Path.IsPathRooted(path)||path.Split('/').Any(x=>x==".."||x==""||x.Contains(":")))throw new Exception("Unsafe resource path");return path;}
    public void Dispose(){if(zip!=null)zip.Dispose();}
}
public class SpriteImage : IDisposable {
    public Frame Frame;public Bitmap Image;
    public void Dispose(){Image.Dispose();}
}
public class ResourceSet : IDisposable {
    List<ResourceLayer> layers=new List<ResourceLayer>();
    Dictionary<string,Dictionary<string,SpriteImage>> cache=new Dictionary<string,Dictionary<string,SpriteImage>>();Queue<string> cacheOrder=new Queue<string>();
    public ResourceSet(string resources,IEnumerable<string> packs){layers.Add(new ResourceLayer(resources));foreach(var pack in packs.Reverse())layers.Add(new ResourceLayer(pack));}
    public byte[] Read(string rel){for(int i=layers.Count-1;i>=0;i--){var b=layers[i].Read(rel);if(b!=null)return b;}throw new Exception("Missing resource: "+rel);}
    public Dictionary<string,SpriteImage> Atlas(string png,bool font=false){
        var result=new Dictionary<string,SpriteImage>(StringComparer.Ordinal);List<Frame> prior=null;
        try{foreach(var layer in layers){var image=layer.Read(png);var meta=layer.Read(Path.ChangeExtension(png,font?".fnt":".plist"));if(image==null&&meta==null)continue;
            if(meta!=null)prior=font?AssetCatalog.FontFrames(meta):Plists.Read(meta);
            if(image==null)throw new Exception("Atlas metadata without matching PNG: "+png);
            if(prior==null)throw new Exception("No atlas metadata: "+png);
            using(var ms=new MemoryStream(image))using(var bitmap=Pictures.Load(ms))foreach(var f in prior){var im=f.Extract(bitmap);SpriteImage old;if(result.TryGetValue(f.Name,out old))old.Dispose();result[f.Name]=new SpriteImage{Frame=f.Copy(),Image=im};}
        }return result;}catch{foreach(var im in result.Values)im.Dispose();throw;}
    }
    public Bitmap Sprite(string png,string frame,bool font=false){string key=png+"|"+font;Dictionary<string,SpriteImage> atlas;if(!cache.TryGetValue(key,out atlas)){atlas=Atlas(png,font);cache[key]=atlas;cacheOrder.Enqueue(key);while(cacheOrder.Count>3){string old=cacheOrder.Dequeue();foreach(var im in cache[old].Values)im.Dispose();cache.Remove(old);}}if(!atlas.ContainsKey(frame))throw new Exception("Missing sprite: "+frame);var source=atlas[frame].Image;return source.Clone(new Rectangle(Point.Empty,source.Size),PixelFormat.Format32bppArgb);}
    public static Dictionary<string,byte[]> PackAtlas(string png,Dictionary<string,SpriteImage> sprites){
        var list=sprites.Values.OrderByDescending(x=>x.Image.Height).ThenByDescending(x=>x.Image.Width).ThenBy(x=>x.Frame.Name,StringComparer.Ordinal).ToList();
        long area=list.Sum(x=>(long)(x.Image.Width+4)*(x.Image.Height+4));int width=64;int min=Math.Max(list.Max(x=>x.Image.Width+4),(int)Math.Ceiling(Math.Sqrt(area)));while(width<min)width*=2;if(width>8192)throw new Exception("Combined atlas exceeds 8192 pixels.");
        int y=2,cursorX=2,row=0;foreach(var s in list){if(cursorX+s.Image.Width+2>width){y+=row+4;cursorX=2;row=0;}s.Frame.X=cursorX;s.Frame.Y=y;s.Frame.W=s.Image.Width;s.Frame.H=s.Image.Height;s.Frame.Rotated=false;cursorX+=s.Image.Width+4;row=Math.Max(row,s.Image.Height);}int height=64;while(height<y+row+2)height*=2;if(height>8192)throw new Exception("Combined atlas exceeds 8192 pixels.");
        using(var bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb)){
            foreach(var s in list)Pictures.CopyExact(s.Image,bitmap,s.Frame.X,s.Frame.Y);
            return new Dictionary<string,byte[]>{{png,Pictures.Png(bitmap)},{Path.ChangeExtension(png,".plist"),Plists.Write(list.Select(s=>s.Frame).ToList(),png,bitmap.Size)}};
        }
    }
    public void Dispose(){foreach(var l in layers)l.Dispose();foreach(var map in cache.Values)foreach(var im in map.Values)im.Dispose();cache.Clear();cacheOrder.Clear();}
}
}
