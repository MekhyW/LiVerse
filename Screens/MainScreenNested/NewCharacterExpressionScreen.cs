﻿using LiVerse.AnabanUI.Controls;
using LiVerse.AnaBanUI;
using LiVerse.AnaBanUI.Containers;
using LiVerse.AnaBanUI.Controls;
using LiVerse.AnaBanUI.Drawables;
using Microsoft.Xna.Framework;

namespace LiVerse.Screens.MainScreenNested {
  public class NewCharacterExpressionScreen {
    UILayer UIRootLayer;

    public NewCharacterExpressionScreen() {
      UIRootLayer = new() { BackgroundRectDrawable = new() { Color = Color.FromNonPremultiplied(0, 0, 0, 127) } };

      DockFillContainer mainDockFillContainer = new() { Margin = new Vector2(40), BackgroundRectDrawable = new() { Color = Color.White } };
      Label titleLabel = new("New Expression", 28, "Ubuntu") { Color = Color.Black };

      LineEdit lineEdit = new("The quick brown fox jumps over the lazy dog The quick brown fox jumps over the lazy dog The quick brown fox jumps over the lazy dog ");

      mainDockFillContainer.DockElement = titleLabel;
      mainDockFillContainer.FillElement = lineEdit;

      UIRootLayer.RootElement = mainDockFillContainer;
    }

    public void ToggleUILayer() {
      if (UIRoot.UILayers.Contains(UIRootLayer)) {
        UIRoot.UILayers.Remove(UIRootLayer);
        return;
      }

      UIRoot.UILayers.Add(UIRootLayer);
    }

  }
}
