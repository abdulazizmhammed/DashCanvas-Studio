// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;using System.Drawing;using System.Windows.Forms;
namespace DashCanvas {
public partial class MainForm {
 ComboBox linePattern=Combo(),textAlign=Combo();NumericUpDown patternSpacing=new NumericUpDown{Minimum=2,Maximum=12,Value=5},gradientAngle=new NumericUpDown{Minimum=0,Maximum=360,Value=90},lineSpacing=new NumericUpDown{Minimum=70,Maximum=200,Value=120},textStroke=new NumericUpDown{Minimum=1,Maximum=8,Value=3};
 CheckBox underline=new CheckBox{Text="Underline",AutoSize=true},strikeThrough=new CheckBox{Text="Strikethrough",AutoSize=true};
 Color frameColor2=Color.DeepPink,textColor2=Color.Cyan,textOutlineColor=Color.FromArgb(5,12,24);Button secondaryFrameButton;
 void BuildAdvancedControls(TabControl tabs){
  var frames=new TabPage("Frame details"){BackColor=Card};var text=new TabPage("Text details"){BackColor=Card};tabs.TabPages.Add(frames);tabs.TabPages.Add(text);
  var f=Table(4,4);Columns(f,100,-1,100,-1);Rows(f,34,34,42,-1);f.Padding=new Padding(7);frames.Controls.Add(f);
  linePattern.Items.AddRange(new object[]{"Solid","Dashed","Dotted","Dash-dot"});linePattern.SelectedIndex=0;AddOption(f,"Line pattern",linePattern,0,0);AddOption(f,"Spacing",patternSpacing,2,0);
  secondaryFrameButton=ColorButton("Second color",frameColor2,delegate(Color c){frameColor2=c;});AddOption(f,"Second color",secondaryFrameButton,0,1);AddOption(f,"Angle",gradientAngle,2,1);
  var presets=Table(5,1);Columns(presets,-1,-1,-1,-1,-1);string[] names={"Sunset","Ice Chrome","Pearl","Candy","Dashed Gold"};for(int i=0;i<names.Length;i++){string name=names[i];var b=Btn(name,false);b.Font=new Font("Segoe UI",9);b.Dock=DockStyle.Fill;b.Click+=delegate{ApplyFramePreset(name);};presets.Controls.Add(b,i,0);}f.Controls.Add(presets,0,2);f.SetColumnSpan(presets,4);
  var note=Label("Line spacing controls gaps. Second color is used by Gradient and Candy Stripe. Angle controls the gradient. All styles follow the picture edge.",Muted);note.Font=new Font("Segoe UI",9);f.Controls.Add(note,0,3);f.SetColumnSpan(note,4);
  var t=Table(4,5);Columns(t,100,-1,100,-1);Rows(t,31,31,31,31,-1);t.Padding=new Padding(7);text.Controls.Add(t);
  textAlign.Items.AddRange(new object[]{"Left","Center","Right"});textAlign.SelectedIndex=1;AddOption(t,"Alignment",textAlign,0,0);AddOption(t,"Line spacing",lineSpacing,2,0);
  AddOption(t,"Second color",ColorButton("Second text color",textColor2,delegate(Color c){textColor2=c;}),0,1);AddOption(t,"Stroke width",textStroke,2,1);
  AddOption(t,"Outline color",ColorButton("Outline color",textOutlineColor,delegate(Color c){textOutlineColor=c;}),0,2);var marks=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};marks.Controls.Add(underline);marks.Controls.Add(strikeThrough);t.Controls.Add(marks,2,2);t.SetColumnSpan(marks,2);
  var textNote=Label("Use Enter in Label text for multiple lines. Alignment and line spacing apply to labels; game-font metrics stay unchanged.",Muted);textNote.Font=new Font("Segoe UI",9);t.Controls.Add(textNote,0,3);t.SetColumnSpan(textNote,4);t.SetRowSpan(textNote,2);
  foreach(var c in new[]{linePattern,textAlign})c.SelectedIndexChanged+=OnDesignStyleChanged;foreach(var c in new[]{patternSpacing,gradientAngle,lineSpacing,textStroke})c.ValueChanged+=OnDesignStyleChanged;underline.CheckedChanged+=OnDesignStyleChanged;strikeThrough.CheckedChanged+=OnDesignStyleChanged;
 }
 void ApplyFramePreset(string name){loading=true;border.Checked=true;matchFrame.Checked=true;frameInset.Value=0;frameWidth.Value=3;patternSpacing.Value=5;linePattern.SelectedItem=name=="Dashed Gold"?"Dashed":"Solid";frameStyle.SelectedItem=name=="Sunset"?"Gradient":name=="Ice Chrome"?"Chrome":name=="Pearl"?"Pearls":name=="Candy"?"Candy Stripe":"Antique Gold";frameColor=name=="Sunset"?Color.Orange:name=="Candy"?Color.Crimson:name=="Dashed Gold"?Color.Gold:Color.LightCyan;frameColor2=name=="Sunset"?Color.DeepPink:Color.White;PaintColor(frameColorButton,frameColor);PaintColor(secondaryFrameButton,frameColor2);loading=false;RefreshPreview();RefreshIdeas();}
 Design AddAdvancedOptions(Design d){d.LinePattern=linePattern.Text;d.PatternSpacing=(float)patternSpacing.Value;d.GradientAngle=(float)gradientAngle.Value;d.FrameColor2=frameColor2;d.TextAlign=textAlign.Text;d.LineSpacing=(float)lineSpacing.Value;d.TextStroke=(float)textStroke.Value;d.TextColor2=textColor2;d.TextOutlineColor=textOutlineColor;d.Underline=underline.Checked;d.StrikeThrough=strikeThrough.Checked;return d;}
 void ShowAbout(){using(var dialog=new Form{Text="About DashCanvas Studio",ClientSize=new Size(540,360),StartPosition=FormStartPosition.CenterParent,BackColor=Bg,ForeColor=Ink,Font=Font,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false}){
  var panel=Table(1,4);panel.Padding=new Padding(22);Rows(panel,45,38,-1,40);dialog.Controls.Add(panel);var title=Label("DashCanvas Studio 3",Mint);title.Font=new Font("Segoe UI",20,FontStyle.Bold);panel.Controls.Add(title,0,0);panel.Controls.Add(Label("Copyright (c) 2026 abdulazizmhammed",Ink),0,1);
  panel.Controls.Add(Label("Licensed under the MIT License.\n\nA local texture designer for Geometry Dash. Game artwork and third-party texture packs belong to their respective owners. This project is not affiliated with or endorsed by RobTop Games.\n\nSource and license: see the files included with this app.",Muted),0,2);var close=Btn("Close",true);close.Dock=DockStyle.Right;close.Width=100;close.Click+=delegate{dialog.Close();};panel.Controls.Add(close,0,3);dialog.AcceptButton=close;dialog.ShowDialog(this);
 }}
}
}
