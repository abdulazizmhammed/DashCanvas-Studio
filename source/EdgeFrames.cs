// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;using System.Drawing;using System.Drawing.Imaging;using System.Runtime.InteropServices;
namespace DashCanvas {
public static partial class Decoration {
 // Measure distance inward from the actual alpha edge, including transparent holes.
 // A frame's outer edge stays fixed when its width, style or glow changes.
 public static void DrawFittedFrame(Bitmap target,Bitmap picture,Design d){
  int w=picture.Width,h=picture.Height,n=checked(w*h);var alpha=new byte[n];var distance=new float[n];
  var data=picture.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
  int left=w,top=h,right=-1,bottom=-1;
  try{var row=new byte[w*4];for(int y=0;y<h;y++){Marshal.Copy(IntPtr.Add(data.Scan0,y*data.Stride),row,0,row.Length);for(int x=0;x<w;x++){int i=y*w+x;alpha[i]=row[x*4+3];bool inside=alpha[i]>16;distance[i]=inside?Math.Min(Math.Min(x+1,w-x),Math.Min(y+1,h-y)):0;if(inside){left=Math.Min(left,x);right=Math.Max(right,x);top=Math.Min(top,y);bottom=y;}}}}
  finally{picture.UnlockBits(data);}if(right<left)return;
  const float diagonal=1.41421356f;
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=y*w+x;if(distance[i]==0)continue;float v=distance[i];if(x>0)v=Math.Min(v,distance[i-1]+1);if(y>0){v=Math.Min(v,distance[i-w]+1);if(x>0)v=Math.Min(v,distance[i-w-1]+diagonal);if(x+1<w)v=Math.Min(v,distance[i-w+1]+diagonal);}distance[i]=v;}
  for(int y=h-1;y>=0;y--)for(int x=w-1;x>=0;x--){int i=y*w+x;if(distance[i]==0)continue;float v=distance[i];if(x+1<w)v=Math.Min(v,distance[i+1]+1);if(y+1<h){v=Math.Min(v,distance[i+w]+1);if(x>0)v=Math.Min(v,distance[i+w-1]+diagonal);if(x+1<w)v=Math.Min(v,distance[i+w+1]+diagonal);}distance[i]=v;}
  float m=Math.Min(right-left+1,bottom-top+1),band=Math.Max(1,m*d.FrameWidth/100f),inset=m*d.FrameInset/100f;
  bool advanced=d.LinePattern!="Solid"||Array.IndexOf(ExtraFrames,d.FrameStyle)>=0;bool neon=d.FrameStyle.Contains("Neon"),wood=d.FrameStyle=="Carved Wood",plain=d.FrameStyle=="Plain";
  data=target.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
  try{var row=new byte[w*4];for(int y=0;y<h;y++){Array.Clear(row,0,row.Length);for(int x=0;x<w;x++){int i=y*w+x;if(alpha[i]<=16)continue;float depth=distance[i]-.5f-inset;if(depth<0)continue;
    Color color=d.FrameColor;float opacity=0;
    if(neon){float center=band*.5f,delta=Math.Abs(depth-center);float core=Math.Max(0,1-delta/Math.Max(.65f,band*.6f));float glow=delta<=band*.55f?0:(float)Math.Exp(-(delta-band*.55f)/Math.Max(1,band*.85f))*.48f*d.Glow/100f;opacity=Math.Max(core,glow);float white=Math.Max(0,1-delta/Math.Max(.5f,band*.18f));color=Blend(color,Color.FromArgb(245,250,255),white);
     if(d.FrameStyle=="Double Neon"){float innerDelta=Math.Abs(depth-band*2.25f);float inner=Math.Max(0,1-innerDelta/Math.Max(.65f,band*.38f));if(inner>opacity){opacity=inner;color=Blend(d.FrameColor,Color.White,Math.Max(0,1-innerDelta/Math.Max(.5f,band*.12f)));}}
    }else if(depth<=band){opacity=Math.Min(1,band-depth+.5f);if(!plain){float t=depth/band;float shade=t<.15f?.42f:t<.3f?1.4f:t<.72f?1f:.55f;if(wood)shade*=.82f+(float)Math.Sin(x*.18+y*.8)*.13f;color=Shade(color,shade);}}
    if(advanced)FrameDetails(d,x,y,left,top,right,bottom,depth,band,ref color,ref opacity);if(opacity<.01f)continue;int p=x*4;row[p]=color.B;row[p+1]=color.G;row[p+2]=color.R;row[p+3]=(byte)Math.Min(alpha[i],Math.Round(255*Math.Min(1,opacity)));
   }Marshal.Copy(row,0,IntPtr.Add(data.Scan0,y*data.Stride),row.Length);}}
  finally{target.UnlockBits(data);}
  if(Rectangular(d.Shape)&&(d.FrameStyle=="Ornate Corners"||d.FrameStyle=="Scrollwork"||wood)){
   float offset=inset+band*.5f;var r=new RectangleF(left+offset,top+offset,right-left+1-2*offset,bottom-top+1-2*offset);
   if(r.Width>3&&r.Height>3)using(var ornaments=new Bitmap(w,h,PixelFormat.Format32bppArgb)){
    using(var g=Graphics.FromImage(ornaments)){g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;Corners(g,r,d.FrameColor,band*.35f,d.FrameStyle=="Scrollwork");}
    ClipToPicture(ornaments,alpha,distance,inset);
    using(var g=Graphics.FromImage(target))g.DrawImageUnscaled(ornaments,0,0);
   }
  }
 }
 static Color Blend(Color a,Color b,float amount){amount=Math.Max(0,Math.Min(1,amount));return Color.FromArgb((int)(a.R+(b.R-a.R)*amount),(int)(a.G+(b.G-a.G)*amount),(int)(a.B+(b.B-a.B)*amount));}
 static void ClipToPicture(Bitmap image,byte[] alpha,float[] distance,float inset){int w=image.Width;var d=image.LockBits(new Rectangle(0,0,w,image.Height),ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);try{var row=new byte[w*4];for(int y=0;y<image.Height;y++){Marshal.Copy(IntPtr.Add(d.Scan0,y*d.Stride),row,0,row.Length);for(int x=0;x<w;x++){int i=y*w+x;row[x*4+3]=distance[i]-.5f<inset?(byte)0:(byte)(row[x*4+3]*alpha[i]/255);}Marshal.Copy(row,0,IntPtr.Add(d.Scan0,y*d.Stride),row.Length);}}finally{image.UnlockBits(d);}}
}
}
