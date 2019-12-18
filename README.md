# NPC-Takeover
Torch Plugin


Description:
This plugin fixes one of my most hated issues with SE and servers

As a server Admin i often find players capture npc grids and take block ownership of the entire grid yet authorship doesnt get transfered too.
So with our ingame server PCU limits players can abuse npc grids and capture hundreds of grids without actually takin authorship of the grid.
 
This plugin checks all control blocks on a pirate ship grid to make sure when a player grinds/takes owner the authorship transfers too. 
Im currently working on finishing up  writing if they are over the limits to keep track of the grid until logout and delete it then.
If over limit, Give warning and say grid will be deleted on log out/Server restart. This will cont. check so if a player transfers it to another player or grinds stuff etc they can keep it
 
I never really found a mod or plugin to do this so I wrote my own
Whats future features do i have in mind:
 - Add UI so server owners can put their own NPC owners in the list. (Right now its only space pirates) I know that some npc mods add more npcs/their own faction
 - Option to completley take block ownership too. Not just authorship
 - Add commands to check what control blocks are left on the grid to grind down to claim ownerships
 - Add admin commands to check currently tracked NPC grids and player grids that are over
 - Tick interval timer UI (How often the checks occur)
 - Im sure ill think of more too (These just came from my other admin friends)

