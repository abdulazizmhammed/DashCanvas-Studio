// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.IO.Compression;

namespace DashCanvas {
public class Design {
    public FitOptions Fit=new FitOptions{Transparent=true};
    public string Shape="Original shape",Symbol="None";
    public int SymbolPosition; // center, bottom-right, top-right
    public bool Border=true;
    public bool SourceIsAsset,ReplaceGlyph,Bold=true,Italic;
    public string Caption="",Glyph="",FontFamily="Segoe UI",TextEffect="Solid",TextPosition="Bottom",FrameStyle="Plain";
    public Color TextColor=Color.White,FrameColor=Color.FromArgb(70,235,255);
    public float TextSize=24,FrameWidth=3,Glow=60;
    public string[] LayerOrder=new[]{"Image","Symbol","Text","Frame"};
    public string[] DeletedLayers=new string[0];
    public Dictionary<string,int> LayerOpacity=new Dictionary<string,int>();
    public bool MatchFrameToImage=true;
    public float FrameInset=0;
    public string LinePattern="Solid",TextAlign="Center";
    public Color FrameColor2=Color.DeepPink,TextColor2=Color.Cyan,TextOutlineColor=Color.FromArgb(5,12,24);
    public float PatternSpacing=5,GradientAngle=90,TextStroke=3,LineSpacing=120;
    public bool Underline,StrikeThrough;
}
public static class Designer {
    static GraphicsPath Shape(string shape,RectangleF r){var p=new GraphicsPath();float d=Math.Min(r.Width,r.Height)*.28f;
        if(shape=="Circle")p.AddEllipse(r);
        else if(shape=="Rounded"){p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();}
        else if(shape=="Diamond")p.AddPolygon(new[]{new PointF(r.X+r.Width/2,r.Y),new PointF(r.Right,r.Y+r.Height/2),new PointF(r.X+r.Width/2,r.Bottom),new PointF(r.X,r.Y+r.Height/2)});
        else if(shape=="Hexagon")p.AddPolygon(new[]{new PointF(r.X+r.Width*.25f,r.Y),new PointF(r.X+r.Width*.75f,r.Y),new PointF(r.Right,r.Y+r.Height*.5f),new PointF(r.X+r.Width*.75f,r.Bottom),new PointF(r.X+r.Width*.25f,r.Bottom),new PointF(r.X,r.Y+r.Height*.5f)});
        else p.AddRectangle(r);return p;
    }
    public static Bitmap Render(Bitmap photo,Bitmap original,Design design) {
        var result=new Bitmap(original.Width,original.Height,PixelFormat.Format32bppArgb);
        try {
            using(var imagePart=RenderLayer(photo,original,design,"Image",RectangleF.Empty))
            using(var g=Graphics.FromImage(result))foreach(string layer in design.LayerOrder.Distinct()) {
                if(design.DeletedLayers.Contains(layer))continue;
                if(design.ReplaceGlyph&&!String.IsNullOrEmpty(design.Glyph)&&(layer=="Image"||layer=="Symbol"))continue;
                int opacity; if(!design.LayerOpacity.TryGetValue(layer,out opacity))opacity=100;
                if(opacity<=0)continue;
                if(layer=="Image")Composite(g,imagePart,opacity);
                else using(var part=RenderLayer(photo,original,design,layer,RectangleF.Empty,layer=="Frame"&&design.MatchFrameToImage?imagePart:null))Composite(g,part,opacity);
            }
            return result;
        }catch{result.Dispose();throw;}
    }
    static void Composite(Graphics g,Bitmap part,int opacity){if(opacity>=100){g.DrawImageUnscaled(part,0,0);return;}using(var attrs=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=opacity/100f;attrs.SetColorMatrix(matrix);g.DrawImage(part,new Rectangle(0,0,part.Width,part.Height),0,0,part.Width,part.Height,GraphicsUnit.Pixel,attrs);}}
    static Bitmap RenderLayer(Bitmap photo,Bitmap original,Design design,string layer,RectangleF frameBounds,Bitmap framePicture=null) {
        var result=layer=="Image"?Pictures.Render(photo,original.Size,design.Fit):new Bitmap(original.Width,original.Height,PixelFormat.Format32bppArgb);
        if(layer=="Image")using(var mask=new Bitmap(original.Width,original.Height,PixelFormat.Format32bppArgb)) {
            using(var g=Graphics.FromImage(mask)){g.SmoothingMode=SmoothingMode.AntiAlias;if(design.Shape=="Original shape")g.DrawImageUnscaled(original,0,0);else using(var p=Shape(design.Shape,new RectangleF(0,0,mask.Width,mask.Height)))g.FillPath(Brushes.White,p);}
            if(!(design.SourceIsAsset&&design.Shape=="Original shape"))MultiplyAlpha(result,mask);
        }
        using(var g=Graphics.FromImage(result)) {
            g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
            float m=Math.Min(result.Width,result.Height);
            if(layer=="Symbol"&&design.Symbol!="None") {
                float size=m*(design.SymbolPosition==0?.48f:.36f),cx=result.Width*.5f,cy=result.Height*.5f;
                if(design.SymbolPosition>0){cx=result.Width-size*.62f;cy=design.SymbolPosition==1?result.Height-size*.62f:size*.62f;}
                var rect=new RectangleF(cx-size/2,cy-size/2,size,size);
                using(var shadow=new SolidBrush(Color.FromArgb(185,8,15,25)))g.FillEllipse(shadow,rect.X-size*.13f,rect.Y-size*.13f,size*1.26f,size*1.26f);
                if(design.Symbol=="Original badge"){g.DrawImage(original,rect);}
                else using(var pen=new Pen(Color.White,Math.Max(1,size*.09f))){pen.StartCap=pen.EndCap=LineCap.Round;
                    if(design.Symbol=="Search"){g.DrawEllipse(pen,rect.X+size*.05f,rect.Y+size*.04f,size*.58f,size*.58f);g.DrawLine(pen,rect.X+size*.57f,rect.Y+size*.57f,rect.Right-size*.03f,rect.Bottom-size*.03f);}
                    else if(design.Symbol=="Play")g.FillPolygon(Brushes.White,new[]{new PointF(rect.X+size*.2f,rect.Y+size*.05f),new PointF(rect.Right-size*.08f,cy),new PointF(rect.X+size*.2f,rect.Bottom-size*.05f)});
                    else if(design.Symbol=="Account"){g.FillEllipse(Brushes.White,cx-size*.2f,rect.Y,size*.4f,size*.4f);g.FillPie(Brushes.White,rect.X,rect.Y+size*.45f,size,size,180,180);}
                    else if(design.Symbol=="Star"){var pts=Enumerable.Range(0,10).Select(i=>{double angle=i*Math.PI/5-Math.PI/2;double radius=size*(i%2==0?.5:.23);return new PointF(cx+(float)(Math.Cos(angle)*radius),cy+(float)(Math.Sin(angle)*radius));}).ToArray();g.FillPolygon(Brushes.White,pts);}
                }
            }
        }
        if(layer=="Text"){
            if(design.ReplaceGlyph&&!String.IsNullOrEmpty(design.Glyph))Decoration.DrawText(result,design.Glyph,design,true);
            else if(!String.IsNullOrWhiteSpace(design.Caption))Decoration.DrawText(result,design.Caption,design,false);
        }
        if(layer=="Frame"&&design.Border&&design.FrameStyle!="None"){if(framePicture!=null)Decoration.DrawFittedFrame(result,framePicture,design);else using(var boundary=new Bitmap(result.Width,result.Height,PixelFormat.Format32bppArgb)){using(var g=Graphics.FromImage(boundary))using(var p=Shape(design.Shape,new RectangleF(0,0,result.Width,result.Height))){g.SmoothingMode=SmoothingMode.AntiAlias;g.FillPath(Brushes.White,p);}Decoration.DrawFittedFrame(result,boundary,design);}}
        return result;
    }
    static void MultiplyAlpha(Bitmap image,Bitmap mask){var rect=new Rectangle(0,0,image.Width,image.Height);var a=image.LockBits(rect,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);var b=mask.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{var ia=new byte[Math.Abs(a.Stride)*a.Height];var ib=new byte[Math.Abs(b.Stride)*b.Height];Marshal.Copy(a.Scan0,ia,0,ia.Length);Marshal.Copy(b.Scan0,ib,0,ib.Length);for(int y=0;y<image.Height;y++)for(int x=0;x<image.Width;x++){int p=y*a.Stride+x*4+3,q=y*b.Stride+x*4+3;ia[p]=(byte)(ia[p]*ib[q]/255);}Marshal.Copy(ia,0,a.Scan0,ia.Length);}finally{image.UnlockBits(a);mask.UnlockBits(b);}}
}
public class EditEntry {
    public string Key,Group,Kind,FrameName;
    public Dictionary<string,string> Images=new Dictionary<string,string>();
}
public class ProjectState {
    public int Version=2;
    public List<EditEntry> Edits=new List<EditEntry>();
    public Dictionary<string,List<string>> Generated=new Dictionary<string,List<string>>();
}
public partial class Game {
    const string ProjectFile="_dashcanvas/project.json";
    public ResourceSet OpenResources(bool includeOwn=false){var paths=Applied(ReadSaved()).Select(EntryPath).Where(p=>includeOwn||!SamePath(p,Pack));return new ResourceSet(Resources,paths);}
    public ProjectState Project(){
        string path=Path.Combine(Pack,ProjectFile);if(File.Exists(path)){var p=Json.Deserialize<ProjectState>(File.ReadAllText(path));if(p==null||p.Version!=2||p.Edits==null||p.Generated==null)throw new Exception("Unsupported DashCanvas project");Validate(p);return p;}
        // Import backgrounds created with version 1 without changing their appearance.
        var state=new ProjectState();if(Directory.Exists(Pack))foreach(var group in Directory.GetFiles(Pack,"game_bg_*.png").GroupBy(x=>AssetCatalog.GroupOf(Path.GetFileName(x)))){
            var e=new EditEntry{Key="image:"+group.Key,Group=group.Key,Kind="image"};foreach(var file in group)e.Images[Path.GetFileName(file)]=Path.GetFileName(file);state.Edits.Add(e);state.Generated[group.Key]=group.Select(Path.GetFileName).ToList();
        }return state;
    }
    void Validate(ProjectState p){foreach(var e in p.Edits){ResourceLayer.SafeRelative(e.Group);foreach(var kv in e.Images){ResourceLayer.SafeRelative(kv.Key);ResourceLayer.SafeRelative(kv.Value);if(!File.Exists(Path.Combine(Pack,kv.Value)))throw new Exception("Missing stored edit: "+kv.Value);}}foreach(var list in p.Generated.Values)foreach(var f in list)ResourceLayer.SafeRelative(f);}
    public Bitmap AssetOriginal(Asset a,string variant,ResourceSet resources){if(a.Kind=="image")using(var ms=new MemoryStream(resources.Read(variant)))return Pictures.Load(ms);return resources.Sprite(variant,a.FrameName,a.Kind=="glyph");}
    public Bitmap AssetCurrent(Asset a,ResourceSet resources){string variant=a.Variants.OrderByDescending(AssetCatalog.Quality).First();return AssetOriginal(a,variant,resources);}
    static string EditName(string key,string variant){using(var h=SHA256.Create())return "_dashcanvas/edits/"+BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(key+"|"+variant))).Replace("-","").ToLowerInvariant()+".png";}
    EditEntry CreateEdit(Asset asset,Bitmap image,Design design,ResourceSet resources,Dictionary<string,byte[]> pending){
        var edit=new EditEntry{Key=asset.Key,Group=asset.Group,Kind=asset.Kind,FrameName=asset.FrameName};
        if(image==null)design.SourceIsAsset=true;
        design.Glyph=asset.Kind=="glyph"?Char.ConvertFromUtf32(Int32.Parse(asset.FrameName.Substring(5))):"";
        using(var active=image==null?OpenResources(true):null)foreach(var variant in asset.Variants){
            if(asset.Kind=="glyph"&&!AssetCatalog.FontFrames(resources.Read(Path.ChangeExtension(variant,".fnt"))).Any(f=>f.Name==asset.FrameName))continue;
            using(var original=AssetOriginal(asset,variant,resources))using(var current=image==null?AssetOriginal(asset,variant,active):null)using(var rendered=Designer.Render(image??current,original,design)){string file=EditName(asset.Key,variant);pending[file]=Pictures.Png(rendered);edit.Images[variant]=file;}}
        return edit;
    }
    byte[] EditBytes(string file,Dictionary<string,byte[]> pending){byte[] bytes;return pending.TryGetValue(file,out bytes)?bytes:File.ReadAllBytes(Path.Combine(Pack,file));}
    Dictionary<string,byte[]> BuildGroup(string group,ProjectState state,ResourceSet resources,Dictionary<string,byte[]> pending){
        var edits=state.Edits.Where(e=>e.Group==group).ToList();var result=new Dictionary<string,byte[]>();if(edits.Count==0)return result;
        if(edits.Any(e=>e.Kind=="image")&&edits.Count>1)throw new Exception("This sheet has individual edits. Restore those edits before replacing the full sheet, or edit an individual asset instead.");
        if(edits[0].Kind=="image") {foreach(var kv in edits[0].Images){result[kv.Key]=EditBytes(kv.Value,pending);foreach(string ext in new[]{".plist",".fnt"}){string rel=Path.ChangeExtension(kv.Key,ext);if(File.Exists(Path.Combine(Resources,rel)))result[rel]=resources.Read(rel);}}return result;}
        foreach(string variant in edits.SelectMany(e=>e.Images.Keys).Distinct()) {
            if(edits[0].Kind=="glyph"){
                using(var ms=new MemoryStream(resources.Read(variant)))using(var sheet=Pictures.Load(ms)){
                    var frames=AssetCatalog.FontFrames(resources.Read(Path.ChangeExtension(variant,".fnt")));
                    using(var g=Graphics.FromImage(sheet)){g.CompositingMode=CompositingMode.SourceCopy;foreach(var e in edits.Where(x=>x.Images.ContainsKey(variant))){var f=frames.FirstOrDefault(x=>x.Name==e.FrameName);if(f==null)throw new Exception("Font character no longer exists: "+e.FrameName);using(var m=new MemoryStream(EditBytes(e.Images[variant],pending)))using(var im=Pictures.Load(m))g.DrawImage(im,f.Stored);}}
                    result[variant]=Pictures.Png(sheet);result[Path.ChangeExtension(variant,".fnt")]=resources.Read(Path.ChangeExtension(variant,".fnt"));
                }continue;
            }
            var atlas=resources.Atlas(variant);
            try{foreach(var e in edits.Where(x=>x.Images.ContainsKey(variant))){SpriteImage item;if(!atlas.TryGetValue(e.FrameName,out item))throw new Exception("Sprite no longer exists: "+e.FrameName);using(var ms=new MemoryStream(EditBytes(e.Images[variant],pending)))using(var im=Pictures.Load(ms)){Bitmap replacement;if(im.Size==item.Image.Size){replacement=new Bitmap(im.Width,im.Height,PixelFormat.Format32bppArgb);Pictures.CopyExact(im,replacement,0,0);}else replacement=Pictures.Render(im,item.Image.Size,new FitOptions{Mode=2,Transparent=true});item.Image.Dispose();item.Image=replacement;}}
                foreach(var kv in ResourceSet.PackAtlas(variant,atlas))result[kv.Key]=kv.Value;
            }finally{foreach(var item in atlas.Values)item.Dispose();}
        }return result;
    }
    public void ApplyAsset(Asset asset,Bitmap image,Design design){
        RequireClosed();if(!HasLoader)throw new Exception("Geode Texture Loader is required. Use Export ZIP instead.");CheckOwner();var state=Project();var pending=new Dictionary<string,byte[]>();
        using(var resources=OpenResources()){var edit=CreateEdit(asset,image,design,resources,pending);if(edit.Images.Count==0)throw new Exception("This character is absent from the active font pack. Choose a character used by that font.");state.Edits.RemoveAll(e=>e.Key==asset.Key);state.Edits.Add(edit);var generated=BuildGroup(asset.Group,state,resources,pending);SaveGroup(asset.Group,state,generated,pending);}
    }
    public void ApplyFont(IEnumerable<Asset> glyphs,Design design){
        RequireClosed();if(!HasLoader)throw new Exception("Geode Texture Loader is required.");CheckOwner();var list=glyphs.Where(a=>a.Kind=="glyph").ToList();if(list.Count==0||list.Select(a=>a.Group).Distinct().Count()!=1)throw new Exception("Select one bitmap font family.");
        design.ReplaceGlyph=true;design.Border=false;design.Caption="";design.Symbol="None";
        var state=Project();var pending=new Dictionary<string,byte[]>();using(var resources=OpenResources()){foreach(var a in list){var edit=CreateEdit(a,null,design,resources,pending);if(edit.Images.Count==0)continue;state.Edits.RemoveAll(e=>e.Key==a.Key);state.Edits.Add(edit);}SaveGroup(list[0].Group,state,BuildGroup(list[0].Group,state,resources,pending),pending);}
    }
    public void RestoreFont(IEnumerable<Asset> glyphs){
        RequireClosed();CheckOwner();var keys=new HashSet<string>(glyphs.Where(a=>a.Kind=="glyph").Select(a=>a.Key));var state=Project();var old=state.Edits.Where(e=>keys.Contains(e.Key)).ToList();if(old.Count==0)throw new Exception("This font has no customized characters.");if(old.Select(e=>e.Group).Distinct().Count()!=1)throw new Exception("Select one bitmap font family.");state.Edits.RemoveAll(e=>keys.Contains(e.Key));var pending=new Dictionary<string,byte[]>();foreach(var e in old)foreach(var file in e.Images.Values)pending[file]=null;using(var resources=OpenResources())SaveGroup(old[0].Group,state,BuildGroup(old[0].Group,state,resources,pending),pending);
    }
    public void RestoreAsset(Asset asset){RequireClosed();CheckOwner();var state=Project();var old=state.Edits.FirstOrDefault(e=>e.Key==asset.Key);if(old==null)throw new Exception("This asset has no DashCanvas design to restore.");state.Edits.Remove(old);var pending=new Dictionary<string,byte[]>();using(var resources=OpenResources()){var generated=BuildGroup(asset.Group,state,resources,pending);foreach(var path in old.Images.Values)if(path.StartsWith("_dashcanvas/"))pending[path]=null;SaveGroup(asset.Group,state,generated,pending);}}
    void SaveGroup(string group,ProjectState state,Dictionary<string,byte[]> generated,Dictionary<string,byte[]> pending){
        var writes=new Dictionary<string,byte[]>();List<string> old;if(state.Generated.TryGetValue(group,out old))foreach(var f in old)if(!generated.ContainsKey(f))writes[Path.Combine(Pack,f)]=null;
        state.Generated[group]=generated.Keys.ToList();foreach(var kv in generated)writes[Path.Combine(Pack,kv.Key)]=kv.Value;foreach(var kv in pending)writes[Path.Combine(Pack,kv.Key)]=kv.Value;
        writes[Path.Combine(Pack,ProjectFile)]=Encoding.UTF8.GetBytes(Json.Serialize(state));writes[Path.Combine(Pack,"pack.json")]=Metadata();
        var data=ReadSaved();var applied=Applied(data);applied.RemoveAll(x=>SamePath(EntryPath(x),Pack));if(state.Edits.Count>0)applied.Insert(0,new Dictionary<string,object>{{"path",Pack}});data["applied"]=applied;writes[Saved]=Encoding.UTF8.GetBytes(Json.Serialize(data));RequireClosed();Transaction.Commit(writes,Backups);
    }
    public void ExportStudio(string path,Asset selected=null,Bitmap image=null,Design design=null){
        var state=Project();var pending=new Dictionary<string,byte[]>();var files=new Dictionary<string,byte[]>();
        if(selected!=null&&design!=null){using(var resources=OpenResources()){var edit=CreateEdit(selected,image,design,resources,pending);if(edit.Images.Count==0)throw new Exception("This character is absent from the active font pack.");state.Edits.RemoveAll(e=>e.Key==selected.Key);state.Edits.Add(edit);foreach(var kv in BuildGroup(selected.Group,state,resources,pending))files[kv.Key]=kv.Value;}}
        foreach(var group in state.Generated)if(selected==null||design==null||group.Key!=selected.Group)foreach(string f in group.Value)files[f]=File.ReadAllBytes(Path.Combine(Pack,f));
        if(files.Count==0)throw new Exception("Choose an asset and a picture, or apply a design first.");files["pack.json"]=Metadata();
        using(var ms=new MemoryStream()){using(var zip=new ZipArchive(ms,ZipArchiveMode.Create,true))foreach(var kv in files)using(var s=zip.CreateEntry(kv.Key,CompressionLevel.Optimal).Open())s.Write(kv.Value,0,kv.Value.Length);Transaction.Atomic(path,ms.ToArray());}
    }
}
}
