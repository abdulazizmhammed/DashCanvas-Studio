// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;using System.Drawing;
namespace DashCanvas {
public static partial class Decoration {
 public static readonly string[] ExtraFrames={"Gradient","Rainbow","Chrome","Copper","Beveled","Double Line","Triple Line","Pearls","Marquee","Candy Stripe"};
 static Color Rainbow(float t){t=(t-(float)Math.Floor(t))*6;int i=(int)t;float f=t-i;int a=(int)(255*f),b=255-a;switch(i){case 0:return Color.FromArgb(255,a,65);case 1:return Color.FromArgb(b,255,65);case 2:return Color.FromArgb(65,255,a);case 3:return Color.FromArgb(65,b,255);case 4:return Color.FromArgb(a,65,255);default:return Color.FromArgb(255,65,b);}}
 static float Cycle(float value){return value-(float)Math.Floor(value);}
 static void FrameDetails(Design d,int x,int y,int left,int top,int right,int bottom,float depth,float band,ref Color color,ref float opacity){
  float nx=(x-left+.5f)/Math.Max(1,right-left+1),ny=(y-top+.5f)/Math.Max(1,bottom-top+1);
  float angle=(float)Math.Atan2(ny-.5f,nx-.5f),travel=(angle+(float)Math.PI)/(2*(float)Math.PI)*(right-left+bottom-top+2)*2;
  if(d.Shape=="Rectangle"||d.Shape=="Rounded"){
   float width=right-left+1,height=bottom-top+1;
   if(Math.Abs(nx-.5f)>Math.Abs(ny-.5f))travel=nx>.5f?width+y-top:2*width+height+bottom-y;
   else travel=ny<.5f?x-left:width+height+right-x;
  }
  float phase=Cycle(travel/Math.Max(4,band*d.PatternSpacing));float t=depth/band;
  switch(d.FrameStyle){
   case "Gradient":double rad=d.GradientAngle*Math.PI/180;float blend=(float)((nx-.5)*Math.Cos(rad)+(ny-.5)*Math.Sin(rad)+.5);color=Blend(d.FrameColor,d.FrameColor2,blend);break;
   case "Rainbow":color=Rainbow((angle+(float)Math.PI)/(2*(float)Math.PI));break;
   case "Chrome":color=Blend(Color.FromArgb(35,48,66),Color.White,(float)(.5+.5*Math.Cos((t-.32f)*Math.PI*2)));break;
   case "Copper":color=Shade(Color.FromArgb(202,116,63),t<.2f?1.5f:t<.7f?1f:.55f);break;
   case "Beveled":color=Shade(d.FrameColor,(nx+ny)<1?1.45f:.5f);break;
   case "Double Line":case "Triple Line":int bands=d.FrameStyle=="Double Line"?2:3;opacity=0;for(int k=0;k<bands;k++)if(Math.Abs(depth-band*(.25f+k*1.25f))<=band*.25f)opacity=1;color=d.FrameColor;break;
   case "Pearls":case "Marquee":float tangent=(phase-.5f)*band*d.PatternSpacing;float radius=band*.6f;float radial=depth-band*.6f;float sphere=(float)Math.Sqrt(tangent*tangent+radial*radial);opacity=Math.Max(0,Math.Min(1,radius-sphere+.5f));color=d.FrameStyle=="Pearls"?Blend(d.FrameColor,Color.White,Math.Max(0,1-sphere/Math.Max(1,radius))):Blend(Color.Orange,d.FrameColor,phase);if(d.FrameStyle=="Marquee"&&depth<band*.12f)opacity=Math.Max(opacity,.55f);break;
   case "Candy Stripe":color=phase<.5f?d.FrameColor:d.FrameColor2;break;
  }
  if(d.LinePattern=="Dashed")opacity*=phase<.65f?1:0;
  else if(d.LinePattern=="Dotted"){float tangent=(phase-.5f)*band*d.PatternSpacing;float radial=depth-band*.5f;opacity*=Math.Max(0,Math.Min(1,band*.6f-(float)Math.Sqrt(tangent*tangent+radial*radial)+.5f));}
  else if(d.LinePattern=="Dash-dot")opacity*=phase<.45f||(phase>.67f&&phase<.8f)?1:0;
 }
}
}
