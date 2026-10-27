// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;using System.Collections.Generic;using System.Drawing;using System.Drawing.Drawing2D;
namespace DashCanvas {
public static partial class Decoration {
 static void LabelPath(GraphicsPath path,string text,FontFamily family,FontStyle style,StringFormat format,Design d){
  string[] lines=text.Replace("\r","").Split('\n');var paths=new List<GraphicsPath>();float widest=0;
  try{foreach(string line in lines){var p=new GraphicsPath();paths.Add(p);if(line.Length>0)p.AddString(line,family,(int)style,100,PointF.Empty,format);if(p.PointCount>0)widest=Math.Max(widest,p.GetBounds().Width);}
   for(int i=0;i<paths.Count;i++){var p=paths[i];if(p.PointCount==0)continue;var b=p.GetBounds();float x=d.TextAlign=="Left"?0:d.TextAlign=="Right"?widest-b.Width:(widest-b.Width)/2;using(var shift=new Matrix()){shift.Translate(x-b.X,i*d.LineSpacing-b.Y);p.Transform(shift);}var r=p.GetBounds();if(d.Underline)p.AddRectangle(new RectangleF(r.Left,r.Bottom+5,r.Width,5));if(d.StrikeThrough)p.AddRectangle(new RectangleF(r.Left,r.Top+r.Height*.48f,r.Width,5));path.AddPath(p,false);}
  }finally{foreach(var p in paths)p.Dispose();}
 }
 static void Relief(Graphics g,GraphicsPath path,float line,bool engraved){for(int i=-1;i<=1;i+=2)using(var p=(GraphicsPath)path.Clone())using(var shift=new Matrix()){shift.Translate(i*line,i*line);p.Transform(shift);bool light=engraved?i>0:i<0;using(var b=new SolidBrush(light?Color.FromArgb(220,255,255,255):Color.FromArgb(210,0,0,0)))g.FillPath(b,p);}}
}
}
