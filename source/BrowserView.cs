// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;using System.Collections.Generic;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;
namespace DashCanvas {
public class StudioButton : Button {
 protected override void OnPaint(PaintEventArgs e){if(Enabled){base.OnPaint(e);return;}using(var b=new SolidBrush(BackColor))e.Graphics.FillRectangle(b,ClientRectangle);using(var p=new Pen(Color.FromArgb(49,65,81)))e.Graphics.DrawRectangle(p,0,0,Width-1,Height-1);TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Color.FromArgb(110,129,147),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);}
}
public class BrowserView : MenuView {
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Color.FromArgb(17,31,48));Hotspots.Clear();int cols=5,rows=3;float cw=(Width-20f)/cols,ch=(Height-28f)/rows;int size=(int)Math.Min(68,Math.Min(cw-18,ch-23));for(int i=0;i<AssetNames.BrowserOrder.Length;i++){
  string name=AssetNames.BrowserOrder[i];var tile=new Rectangle(10+(int)(i%cols*cw),4+(int)(i/cols*ch),(int)cw-6,(int)ch-4);Hotspots[name]=tile;
  using(var b=new SolidBrush(name==SelectedName?Color.FromArgb(37,66,77):Color.FromArgb(26,45,65)))g.FillRectangle(b,tile);
  Bitmap im;if(name==SelectedName&&Pending!=null)im=Pending;else Images.TryGetValue(name,out im);
  if(im!=null){float s=Math.Min((float)size/im.Width,(float)size/im.Height);int w=Math.Max(1,(int)(im.Width*s)),h=Math.Max(1,(int)(im.Height*s));g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(im,tile.X+(tile.Width-w)/2,tile.Y+3,w,h);}
  using(var font=new Font("Segoe UI",9,FontStyle.Bold))TextRenderer.DrawText(g,AssetNames.Display(name,"sprite",""),font,new Rectangle(tile.X,tile.Bottom-22,tile.Width,20),Color.White,TextFormatFlags.HorizontalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
  if(name==SelectedName)using(var p=new Pen(Color.FromArgb(93,241,197),2))g.DrawRectangle(p,tile);
 }TextRenderer.DrawText(g,"Click a button to customize it. Layout is illustrative.",Font,new Rectangle(0,Height-23,Width,23),Color.FromArgb(151,169,187),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}
}
public partial class MainForm {
 BrowserView browser=new BrowserView();
 void DrawGalleryItem(object sender,DrawListViewItemEventArgs e){
  var r=new Rectangle(e.Bounds.X+3,e.Bounds.Y+3,e.Bounds.Width-6,e.Bounds.Height-6);bool selected=Selected!=null&&((Asset)e.Item.Tag).Key==Selected.Key;
  using(var b=new SolidBrush(selected?Color.FromArgb(35,58,68):Color.FromArgb(28,39,54)))e.Graphics.FillRectangle(b,r);
  if(selected)using(var p=new Pen(Mint,2))e.Graphics.DrawRectangle(p,r);
  var image=thumbnails.Images[e.Item.ImageKey];if(image!=null)e.Graphics.DrawImage(image,r.X+(r.Width-image.Width)/2,r.Y+4,image.Width,image.Height);
  using(var f=new Font("Segoe UI",9,FontStyle.Bold))TextRenderer.DrawText(e.Graphics,e.Item.Text,f,new Rectangle(r.X+4,r.Y+87,r.Width-8,r.Height-89),Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
 }
}
}
