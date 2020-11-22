# fivem-prop-hunt
 
BUGS:
1. cl_World.Cleanup seems to fail when the player is not yet in the area to be cleaned up. I am assuming that the player needs to load that area of the world prior to deleting props in that area.

TODO:
1. Need to add locations or "sections" of the map to play on
2. Prop rotation replication is not working
3. Add actual player hud to game.
        Name
        Scoreboard
        Health/Armor
        Idle indicator until next taunt
        
TUNING:
1. Adjust spectator camera's sentitivity. Maybe use player's configured sensitivity.
2. Spectator camera doesn't show player's gamertag if they are outside of the range of the spectator's body.
3. Add new taunt audio clips.
4. Optimize prop highlighting. Appears to be a massive FPS drop around a lot of props.