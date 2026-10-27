// Copyright (c) 2026 abdulazizmhammed. Licensed under the MIT License.
using System;using System.Linq;using System.Collections.Generic;using System.Drawing;using System.Windows.Forms;
namespace DashCanvas {
public partial class MainForm {
 readonly List<string> layerOrder=new List<string>{"Image","Symbol","Text","Frame"};
 readonly HashSet<string> deletedLayers=new HashSet<string>();
 ComboBox layerPicker=Combo();Button layerForward,layerBackward,layerDelete,layerRestore;bool updatingLayers;
 readonly Dictionary<string,int> layerOpacity=new Dictionary<string,int>();
 NumericUpDown opacityControl=new NumericUpDown{Minimum=0,Maximum=100,Value=100};Button layerMore;
 string selectedLayer="Text";
 void BuildLayerControls(TableLayoutPanel views){
  views.Controls.Remove(preview);var pane=Table(1,2);Rows(pane,-1,66);views.Controls.Add(pane,1,1);pane.Controls.Add(preview,0,0);
  var tools=Table(1,2);Rows(tools,30,36);pane.Controls.Add(tools,0,1);
  var selection=Table(3,1);Columns(selection,-1,68,57);tools.Controls.Add(selection,0,0);layerPicker.Dock=DockStyle.Fill;layerPicker.AccessibleName="Design layer, front to back";selection.Controls.Add(layerPicker,0,0);selection.Controls.Add(Label("Opacity",Muted),1,0);opacityControl.Dock=DockStyle.Fill;opacityControl.AccessibleName="Layer opacity percent";selection.Controls.Add(opacityControl,2,0);opacityControl.ValueChanged+=delegate{if(updatingLayers)return;layerOpacity[selectedLayer]=(int)opacityControl.Value;RefreshPreview();};
  var actions=Table(5,1);Columns(actions,-1,-1,-1,-1,60);tools.Controls.Add(actions,0,1);
  layerForward=Btn("Forward",false);layerBackward=Btn("Back",false);layerDelete=Btn("Delete",false);layerRestore=Btn("Restore",false);
  var buttons=new[]{layerForward,layerBackward,layerDelete,layerRestore};for(int i=0;i<buttons.Length;i++){buttons[i].Dock=DockStyle.Fill;buttons[i].Font=new Font("Segoe UI",8);actions.Controls.Add(buttons[i],i,0);}
  layerMore=Btn("More",false);layerMore.Dock=DockStyle.Fill;layerMore.Font=new Font("Segoe UI",8);actions.Controls.Add(layerMore,4,0);var menu=new ContextMenuStrip();var front=menu.Items.Add("Bring to front");var back=menu.Items.Add("Send to back");menu.Items.Add(new ToolStripSeparator());var reset=menu.Items.Add("Reset layer order and opacity");front.Click+=delegate{layerOrder.Remove(selectedLayer);layerOrder.Add(selectedLayer);RefreshPreview();RefreshIdeas();};back.Click+=delegate{layerOrder.Remove(selectedLayer);layerOrder.Insert(0,selectedLayer);RefreshPreview();RefreshIdeas();};reset.Click+=delegate{layerOrder.Clear();layerOrder.AddRange(new[]{"Image","Symbol","Text","Frame"});layerOpacity.Clear();RefreshPreview();RefreshIdeas();};menu.Opening+=delegate{front.Enabled=layerForward.Enabled;back.Enabled=layerBackward.Enabled;};layerMore.Click+=delegate{menu.Show(layerMore,new Point(0,layerMore.Height));};Disposed+=delegate{menu.Dispose();};
  var tips=new ToolTip();tips.SetToolTip(layerPicker,"Layers are listed front to back. The existing game texture is one image layer.");tips.SetToolTip(layerForward,"Bring the selected layer forward by one visible layer.");tips.SetToolTip(layerBackward,"Send the selected layer backward by one visible layer.");tips.SetToolTip(layerDelete,"Remove this layer from the preview. Apply selected saves the result.");tips.SetToolTip(layerRestore,"Restore the deleted layer at its previous position.");Disposed+=delegate{tips.Dispose();};
  layerPicker.SelectedIndexChanged+=delegate{if(updatingLayers||layerPicker.SelectedIndex<0)return;selectedLayer=layerOrder[layerOrder.Count-1-layerPicker.SelectedIndex];RefreshLayerControls();};
  layerForward.Click+=delegate{MoveDesignLayer(1);};layerBackward.Click+=delegate{MoveDesignLayer(-1);};
  layerDelete.Click+=delegate{SetLayerDeleted(true);};layerRestore.Click+=delegate{SetLayerDeleted(false);};
  layerPicker.KeyDown+=delegate(object s,KeyEventArgs e){if(e.KeyCode==Keys.Delete&&layerDelete.Enabled){SetLayerDeleted(true);e.Handled=true;}};
  RefreshLayerControls();
 }
 bool LayerHasContent(string name){
  if(Selected==null)return false;
  if(name=="Image")return !replaceGlyph.Checked;
  if(name=="Symbol")return symbol.Text!="None"&&!replaceGlyph.Checked;
  if(name=="Text")return replaceGlyph.Checked||!String.IsNullOrWhiteSpace(caption.Text);
  return border.Checked&&frameStyle.Text!="None";
 }
 int LayerNeighbor(int direction){int i=layerOrder.IndexOf(selectedLayer)+direction;while(i>=0&&i<layerOrder.Count){string name=layerOrder[i];if(!deletedLayers.Contains(name)&&LayerHasContent(name))return i;i+=direction;}return -1;}
 void RefreshLayerControls(){
  if(layerForward==null)return;updatingLayers=true;layerPicker.BeginUpdate();layerPicker.Items.Clear();
  foreach(string name in layerOrder.AsEnumerable().Reverse())layerPicker.Items.Add(name+" layer"+(deletedLayers.Contains(name)?" (deleted)":!LayerHasContent(name)?" (empty)":"") );
  layerPicker.SelectedIndex=layerOrder.Count-1-layerOrder.IndexOf(selectedLayer);int opacity;opacityControl.Value=layerOpacity.TryGetValue(selectedLayer,out opacity)?opacity:100;layerPicker.EndUpdate();updatingLayers=false;
  bool ready=Selected!=null,deleted=deletedLayers.Contains(selectedLayer),content=LayerHasContent(selectedLayer);
  layerPicker.Enabled=ready;layerDelete.Enabled=ready&&!deleted&&content;layerRestore.Enabled=ready&&deleted;
  opacityControl.Enabled=ready&&!deleted&&content;layerMore.Enabled=ready;
  layerForward.Enabled=ready&&!deleted&&content&&LayerNeighbor(1)>=0;layerBackward.Enabled=ready&&!deleted&&content&&LayerNeighbor(-1)>=0;
 }
 void MoveDesignLayer(int direction){
  int neighbor=LayerNeighbor(direction);if(Selected==null||deletedLayers.Contains(selectedLayer)||neighbor<0)return;
  layerOrder.Remove(selectedLayer);layerOrder.Insert(neighbor,selectedLayer);RefreshPreview();RefreshIdeas();Status(selectedLayer+" layer moved "+(direction>0?"forward.":"backward."),true);
 }
 void SetLayerDeleted(bool deleted){
  if(Selected==null)return;if(deleted)deletedLayers.Add(selectedLayer);else deletedLayers.Remove(selectedLayer);
  RefreshPreview();RefreshIdeas();Status(selectedLayer+(deleted?" layer deleted from this design. Use Restore to bring it back.":" layer restored."),true);
 }
 Design AddLayerOptions(Design d){d.LayerOrder=layerOrder.ToArray();d.DeletedLayers=deletedLayers.ToArray();d.LayerOpacity=new Dictionary<string,int>(layerOpacity);d.MatchFrameToImage=matchFrame.Checked;d.FrameInset=(float)frameInset.Value;return AddAdvancedOptions(d);}
}
}
