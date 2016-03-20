package com.apps.kruszyn.lightorganapp;

/**
 * Created by nazyw on 3/19/2016.
 */
public class MediaFileItem {
    public final String title;
    public final String artist;
    public final int duration;
    public final String name;

    public MediaFileItem(String title, String artist, int duration, String name)
    {
        this.title = title;
        this.artist = artist;
        this.duration = duration;
        this.name = name;
    }
}
