﻿using LiVerse.AnaBanUI;
using LiVerse.AnaBanUI.Containers;
using LiVerse.AnaBanUI.Controls;
using LiVerse.AnaBanUI.Drawables;
using LiVerse.AnaBanUI.Events;
using LiVerse.CaptureDeviceDriver;
using LiVerse.CaptureDeviceDriver.WasapiCaptureDevice;
using LiVerse.CharacterRenderer;
using LiVerse.Screens.MainScreenNested;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LiVerse.Screens
{
    public class MainScreen : ScreenBase {
    SettingsScreen settingsScreen { get; set; }
    UILayer WindowRoot;

    // MainUI Members
    DockFillContainer mainFillContainer = new();
    VerticalLevelTrigger micLevelTrigger;
    VerticalLevelTrigger levelDelayTrigger;
    DockFillContainer HeaderBar;
    DockFillContainer centerSplit;
    CharacterRenderer.CharacterRenderer characterRenderer;
    DockFillContainer sideFillContainer;
    SolidColorRectangle speakingIndicatorSolidColorRect;
    Label speakingIndicatorLabel;
    Label characterNameLabel;

    KeyboardState oldState;
    bool characterFullView = false;

    // Static ReadOnly Fields
    static readonly Color speakingIndicatorColor = Color.FromNonPremultiplied(8, 7, 5, 50);
    static readonly Color speakingIndicatorActiveColor = Color.FromNonPremultiplied(230, 50, 75, 255);
    static readonly Color speakingIndicatorLabelColor = Color.FromNonPremultiplied(255, 255, 255, 50);
    static readonly Color speakingIndicatorActiveLabelColor = Color.FromNonPremultiplied(255, 255, 255, 255);

    public MainScreen(ScreenManager screenManager) : base(screenManager) {
      WindowRoot = new UILayer();
      settingsScreen = new SettingsScreen();      

      HeaderBar = new DockFillContainer();
      centerSplit = new DockFillContainer();
      // Create CharacterRenderer
      characterRenderer = new CharacterRenderer.CharacterRenderer();

      characterNameLabel = new Label("{character_name}", 21);
      characterNameLabel.Color = Color.Black;
      Button settingsButton = new Button("Settings", 21);
      settingsButton.Click += settingsScreen.ToggleUILayer;

      micLevelTrigger = new VerticalLevelTrigger();
      levelDelayTrigger = new VerticalLevelTrigger();
      micLevelTrigger.MaximumValue = 84;
      micLevelTrigger.ShowPeaks = true;
      levelDelayTrigger.MaximumValue = 1;

      SideBySideContainer sideBySide = new SideBySideContainer();
      sideFillContainer = new DockFillContainer();
      sideFillContainer.Margin = 4f;

      sideBySide.Elements.Add(micLevelTrigger);
      sideBySide.Elements.Add(levelDelayTrigger);
      sideBySide.Gap = 4f;

      sideFillContainer.DockType = DockFillContainerDockType.Bottom;
      sideFillContainer.FillElement = sideBySide;

      speakingIndicatorLabel = new Label("Active", 21);
      speakingIndicatorLabel.Color = speakingIndicatorLabelColor;
      speakingIndicatorSolidColorRect = new SolidColorRectangle(speakingIndicatorLabel);
      speakingIndicatorSolidColorRect.Margin = 4f;
      speakingIndicatorSolidColorRect.BackgroundColor = speakingIndicatorColor;
      
      sideFillContainer.DockElement = speakingIndicatorSolidColorRect;

      HeaderBar.DockType = DockFillContainerDockType.Left;
      HeaderBar.BackgroundRectDrawble = new RectangleDrawable() { Color = Color.FromNonPremultiplied(249, 249, 249, 255), IsFilled = true };
      HeaderBar.DockElement = settingsButton;
      HeaderBar.FillElement = characterNameLabel;
      
      centerSplit.DockType = DockFillContainerDockType.Left;
      centerSplit.DockElement = sideFillContainer;
      centerSplit.FillElement = characterRenderer;

      //HeaderBar.Lines = true;
      mainFillContainer.DockElement = HeaderBar;
      mainFillContainer.FillElement = centerSplit;

      WindowRoot.RootElement = mainFillContainer;


      CaptureDeviceDriverManager.CaptureDeviceDriver.MicrophoneLevelTriggered += MicrophoneLevelMeter_CharacterStartSpeaking;
      CaptureDeviceDriverManager.CaptureDeviceDriver.MicrophoneLevelUntriggered += MicrophoneLevelMeter_CharacterStopSpeaking;
      CaptureDeviceDriverManager.CaptureDeviceDriver.MicrophoneVolumeLevelUpdated += MicrophoneLevelMeter_MicrophoneVolumeLevelUpdate;


      CaptureDeviceDriverManager.CaptureDeviceDriver.Initialize();
      CaptureDeviceDriverManager.CaptureDeviceDriver.SetDefaultDevice();

      WindowRoot.InputUpdateEvent += FullscreenViewToggle;

      // Registers WindowRoot UILayer
      UIRoot.UILayers.Add(WindowRoot);
      settingsScreen.ToggleUILayer();
    }

    private void MicrophoneLevelMeter_CharacterStopSpeaking() {
      speakingIndicatorSolidColorRect.BackgroundColor = speakingIndicatorColor;
      speakingIndicatorLabel.Color = speakingIndicatorLabelColor;

      characterRenderer.SetSpeaking(false);
    }

    private void MicrophoneLevelMeter_CharacterStartSpeaking() {
      speakingIndicatorSolidColorRect.BackgroundColor = speakingIndicatorActiveColor;
      speakingIndicatorLabel.Color = speakingIndicatorActiveLabelColor;

      characterRenderer.SetSpeaking(true);
    }

    private void MicrophoneLevelMeter_MicrophoneVolumeLevelUpdate(double value) {
      micLevelTrigger.CurrentValue = (float)value;
    }


    public override void Deattach() { }

    public override void Dispose() {
      CaptureDeviceDriverManager.CaptureDeviceDriver.Dispose();
    }

    public override void Draw(SpriteBatch spriteBatch, double deltaTime) {
      if (!characterFullView) {
        spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
      }else {
        spriteBatch.GraphicsDevice.Clear(Color.Transparent);
      }

      UIRoot.DrawUILayers(spriteBatch, deltaTime);
    }

    public override void Update(double deltaTime) {
      // Set Values
      levelDelayTrigger.CurrentValue = (float)CaptureDeviceDriverManager.CaptureDeviceDriver.ActivationDelay;

      // Sincronize Values
      micLevelTrigger.TriggerLevel = CaptureDeviceDriverManager.CaptureDeviceDriver.TriggerLevel;
      micLevelTrigger.MaximumValue = CaptureDeviceDriverManager.CaptureDeviceDriver.MaximumLevel;
      levelDelayTrigger.TriggerLevel = CaptureDeviceDriverManager.CaptureDeviceDriver.ActivationDelayTrigger;

      UIRoot.UpdateUILayers(deltaTime);

      // Set CharacterName Label
      if (characterRenderer.CurrentCharacter != null) {
        characterNameLabel.Text = characterRenderer.CurrentCharacter.Name;
      }else { characterNameLabel.Text = "No character selected"; }

      // Sincronize Changes
      CaptureDeviceDriverManager.CaptureDeviceDriver.TriggerLevel = micLevelTrigger.TriggerLevel;
      CaptureDeviceDriverManager.CaptureDeviceDriver.ActivationDelayTrigger = levelDelayTrigger.TriggerLevel;

      CaptureDeviceDriverManager.CaptureDeviceDriver.Update(deltaTime);

      HeaderBar.Visible = !characterFullView;
      if (centerSplit.DockElement != null) centerSplit.DockElement.Visible = !characterFullView;
    }

    void FullscreenViewToggle(KeyboardEvent keyboardEvent) {
      // Check if toggle key has been pressed
      if (keyboardEvent.NewKeyboardState.IsKeyDown(Keys.Escape) && keyboardEvent.OldKeyboardState.IsKeyUp(Keys.Escape)) {
        characterFullView = !characterFullView;
      }
    }

  }
}
