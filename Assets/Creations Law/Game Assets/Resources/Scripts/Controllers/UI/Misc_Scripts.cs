﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SavePackage;

//###################################//
//                                                                              //
//            UI Controls                                                 //
//                                                                            //
//                                                                           //
//#################################//
//                                                                         //
//    A script responsible for the scrolling of menus.//
//                                                                     //
//                                                                    //
/////////////////////////////////////////////////////////






    public class UIControls : MonoBehaviour
    {

        public int selectedItem;
        protected GameManager game;


        protected virtual void Start()
        {
            game = FindObjectOfType<GameManager>();
            if (game.isGamepad) //Determines if a gamepad is plugged in
            {
            selectedItem = 0;
            }
        }
        protected virtual void Update()
        {
            if (game.controller == "Keyboard")
            {
                Cursor.visible = true;
            }
            if (game.controller != "Keyboard")
            {
                Cursor.visible = false;
            }
        }

        /// <summary>
        ///  The main controls behind menu scrolling on the pause menu.
        /// </summary>
        /// <param name="min">The minimum value of what the menu can count down to.</param>
        /// <param name="max">The maximum value of what the menu can count down to.</param>
        public virtual void UpDownHandler(int max, int min)
        {
            if (game.isGamepad)
            {
                if (game.direction == "up")
                {
                    if (game.d_Up == false)
                    {
                        game.d_Up = true;
                        selectedItem--;
                    }
                }
                if (game.direction == "down")
                {
                    if (game.d_Down == false)
                    {
                        game.d_Down = true;
                        selectedItem++;
                    }
                }
            }

        }
        /// <summary>
        /// If the value goes over or under a certain threshold, select the respective max or min value.
        /// </summary>
        /// <param name="min">The minimum value of what the menu can count down to.</param>
        /// <param name="max">The maximum value of what the menu can count down to.</param>
        protected virtual void MenuScroller(int min, int max)
        {
            if (game.isGamepad)
            {
                if (selectedItem < min)
                {
                    selectedItem = max;
                }
                if (selectedItem > max)
                {
                    selectedItem = min;
                }
            }
        }
    }



    //###################################//
    //                                                                              //
    //            REPLACEMENT MOUSEOVER                         //
    //                                                                            //
    //                                                                           //
    //#################################//
    //                                                                         //
    //    A better script for mousing over                      //
    //    than the default one that unity provides.         //
    //                                                                     //
    //                                                                    //
    /////////////////////////////////////////////////////////


    /// <summary>
    /// A replacement script for when the mouse cursor is hovering over a button.
    /// </summary>
    public class MouseOverButton : MonoBehaviour
    {
        public PauseMenu pause;
        public ChangeScene title;
        public Level_Select level;
        public SaveMenu save;
        public int buttonNumber;

        void Start()
        {
            title = FindObjectOfType<ChangeScene>();
            pause = FindObjectOfType<PauseMenu>();
            level = FindObjectOfType<Level_Select>();
            save = FindObjectOfType<SaveMenu>();
        }

        public void Switcher()
        {
            if (title != null)
                title.SendMessage("MouseMenu", buttonNumber);

            if (pause != null)
                pause.SendMessage("MouseMenu", buttonNumber);

            if (level != null)
                level.SendMessage("MouseMenu", buttonNumber);

            if (save != null)
                save.SendMessage("MouseMenu", buttonNumber);

        }
        public void MenuOff()
        {
            if (title != null)
                title.SendMessage("MouseMenu", buttonNumber);

            if (pause != null)
                pause.SendMessage("MouseMenu", -100);

            if (level != null)
                level.SendMessage("MouseMenu", -100);

            if (save != null)
                save.SendMessage("MouseMenu", 4);
        }
    }

    public class Misc_Scripts : MouseOverButton
    {

    }
