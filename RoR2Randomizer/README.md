# Risk of Rain 2 Randomizer

A randomizer mod for Risk of Rain 2!

It is *highly* recommended to take a look over the mod settings before starting a run. By default most randomizers are enabled, which can be a bit much to take in at once.

Current features:
* Multiplayer Compatible. Every player needs to have the mod.
* Compatibile with Risk of Options for easy configuration in-game.
* Stage Randomizer: Swaps every stage for a random playable map.
  * Each stage is paired one-to-one with a replacement stage (for example bazaar will always be randomized to the same stage during the course of a run)
* Boss Randomizer: Randomizes the character types of bosses. Currently [Mithrix](https://youtu.be/EZLbivjyFMk), [Voidling](https://youtu.be/HAH9Rxyl2lU), Aurelionite, Twisted Scavengers and Alloy Worship Unit are implemented.
* Status Effect Randomizer: Randomizes all buffs/debuffs applied to characters.
* Survivor Starting Animation Randomizer: Randomizes the first stage spawn animation of all survivors. ![randomspawnpod](https://cdn.discordapp.com/attachments/526159007442927648/1025178769675264061/randomspawnpod.gif)
* Projectile Randomizer: Randomizes projectiles.
* Summon Randomizer: Randomizes summoned characters.
  * Buying drones
  * Squid Polyp
  * Engineer Turrets
  * Soul Wisp
  * Malachite Urchin
  * Healing Core
  * Void Infestors
  * Beetle Guard, both from Queens Gland and spawned by the Beetle Queen itself.
  * Quiet Probe and Delighted Probe spawned from the Solus Probes item
  * Strike Drone spawned from The back-up equipment
  * Col. Droneman spawned from Spare Drone Parts
  * Shopkeeper Newt
  * Solus Probes spawned by Solus Control Unit and Alloy Worship Unit
  * Heretic
  * Alpha Construct spawned by Defense Nucleus
* Effect Randomizer: Randomizes visual effects.
* Weak Point Randomizer: Randomizes which hitboxes are considered weak points by Railgunner's scoped shot. The number of weak points per character remain the same.

Questions, Bug Reports, Feedback? Please DM me on Discord: Gorakh#0821

## Changelog

**0.7.1 Changes:**

* Projectile Randomizer:
  * Added Spite Bombs to Projectile Randomizer
  * Added option to exclude instakill projectiles from the randomizer, disabled by default.
  * Randomized bullets now inherit the max range of the original bullet.
  * Fixed certain projectile types having infinite lock-on range.

* Status Effect Randomizer:
  * Excluded an "invisible" (no icon, has no effect) status effect from the randomizer.

**0.7.0 Changes:**

* Projectile Randomizer:
  * Added more types of projectiles to the randomization pool:
    * Huntress primaries
    * Squid turret projectile
    * Plasma shrimp projectile
    * Royal capacitor
    * Huntress Glaive
    * Ukulele
    * Preon zaps
    * Razorwire
    * Acrid Epidemic spreading
    * Probably a few more minor ones I've forgotten to list
  * Excluded a projectile that does nothing.

* Status Effect Randomizer:
  * Fixed Void Fiend Trespass (Utility Skill) cleansing the corrupted status effect if it is randomized into a DOT, resulting in permanent corruption mode.

* Summon Randomizer:
  * Fixed randomized summons sometimes changing mid-run.

**0.6.0 Changes:**

* Projectile Randomizer:
  * Added option to randomize hitscan attacks (enabled by default), meaning any hitscan attack (primarily bullets) can become a projectile or another type of bullet, regular projectiles can also become bullets.
    * You thought there were survivors "safe" from the randomizer? Think again! (Unless you're thinking of Huntress, then you are still correct)

* Summon Randomizer:
  * Added Alpha Construct spawned by Defense Nucleus to summon randomizer.
  * The names in the descriptions of the Squid Polyp and Defense Nucleus items are now replaced with the name of that character's replacement.
  * (Possibly) fixed summon replacements sometimes changing mid-run

**0.5.10 Changes:**

* Summon Randomizer:
  * Fixed randomized Heretic character getting all abilities replaced with "Nevermore" (ex. if Heretic is randomized into Lemurian, then all Lemurians during that run would get Nevermore in all of their skill slots)
  * Replaced Heretic name in Mithrix dialogue. ![mithrixhereticname](https://cdn.discordapp.com/attachments/526159007442927648/1037385824511197294/unknown.png)

**0.5.9 Changes:**

* Stage Randomizer:
  * Fixed out of bounds triggers on ai_test not killing teleporter bosses

**0.5.8 Changes:**

* Summon Randomizer:
  * Added Heretic to Summon randomizer (Getting all heresy items will turn you into a random character instead of Heretic)

**0.5.7 Changes:**

* Stage Randomizer:
  * Fixed Old Commencement softlocking with no way out of the arena after defeating Mithrix.

* Gup Mode:
  * Added Gup Mode.

**0.5.6 Changes:**

* Boss Randomizer:
  * Fixed Voidling replacing Mithrix sometimes spawning in the ground.

* Projectile Randomizer:
  * Fixed game sometimes freezing indefinitely after an Overloading character attacks.

**0.5.5 Changes:**

* Status Effect Randomizer:
  * Fixed Randomized Timed Buffs not ending.

**0.5.4 Changes:**

* Summon Randomizer:
  * Added Void allies spawned by Newly Hatched Zoea to Summon Randomizer.

* Status Effect Randomizer:
  * Added option to control how likely a buff is to be randomized into a debuff and vice versa.

* Mod Compatibility:
  * Improved [DropInMultiplayer](https://thunderstore.io/package/niwith/DropinMultiplayer/) Compatibility

**0.5.3 Changes:**

* Stage Randomizer:
  * Added stages to pool:
    * Old Commencement
    * ai_test
    * testscene

* Summon Randomizer:
  * Fixed Grandparents replacing Engineer turrets pushing the user into the ground

* Weak Point Randomizer:
  * Added Weak Point Randomizer.

* Mod Compatibility:
  * Added [DropInMultiplayer](https://thunderstore.io/package/niwith/DropinMultiplayer/) Compatibility

**0.5.2 Changes:**

* Status Effect Randomizer:
  * Fixed DOTs randomizing into non-DOT effects being permanent

* Misc:
  * Fixed some incorrect dependency package references

**0.5.1 Changes:**

* Performance:
  * Added options to limit how much characters can self-duplicate themselves (no more infinite Goobo spawns yay)
  * Decreased size of most network messages sent by the mod

**0.5.0 Changes:**

* Effect Randomizer:
  * Added Effect Randomizer

* Status Effect Randomizer:
  * Fixed DOT status effects not being properly randomized. (An effect randomized into bleed did not inflict bleed, and if bleed was randomized into a non-DOT effect, bleed would still be applied)
  * Fixed Safer Spaces never going on cooldown
  * Fixed Shurikens giving a new stack of buff every frame
  * Fixed Void Fiend getting stuck at 100% corruption if status effect randomizer was enabled

* Summon Randomizer:
  * Added Shopkeeper Newt to Summon Randomizer
  * Added Solus Probes to Summon Randomizer

* Misc:
  * Added an info message on first startup
  * Added Risk of Options as a required dependency

**0.4.0 Changes:**

* Summon Randomizer:
  * Added Summon Randomizer

**0.3.5 Changes:**

* Boss Randomizer:
  * Twisted Scavengers can now be randomized.
  * Alloy Worship Unit on Siren's Call can now be randomized.
  * Fixed Aurelionite not spawning during Mithrix fight if Mithrix randomizer is enabled

* Projectile Randomizer:
  * Disabled randomization of the Egocentrism projectile.
    * This will be re-enabled again at some point in the future, but I just can't deal with this edge case right now.

**0.3.4 Changes:**

* Boss Randomizer:
  * Fixed Mithrix not dropping the Halcyon Seed if he replaces Aurelionite

**0.3.3 Changes:**

* Boss Randomizer:
  * Added Aurelionite to boss randomizer
    * Also randomizes character spawned by Halcyon Seed during the teleporter event

* Projectile Randomizer:
  * Tweaked how Artificers walls are affected by the projectile randomizer.

**0.3.2 Changes:**

* Status Effect Randomizer:
  * Fixed stacking effects not working
  * Added some missing invincibility buffs to the invincibility status effect list

**0.3.1 Changes:**

* Projectile Randomizer:
  * Fixed Grovetender chains not working on other characters
  * Fixed Loader grapple not working with projectile randomizer
  * Excluded various projectiles that do nothing

**0.3.0 Changes:**

* Added Projectile Randomizer.

**0.2.0 Changes:**

* Added Survivor Starting Animation Randomizer

* Status Effect Randomizer
  * Added option to exclude invincibility buffs from the randomizer to prevent potential softlocks

**0.1.0 Changes:**

* Added Status Effect Randomizer.
  * Includes option to disable buffs turning into debuffs and vice verse

* Mod Compatibility:
  * Added an icon in the Risk of Options settings menu

**0.0.2 Changes:**

* Stage Randomizer:
  * Added a weight system to decrease the likelyhood of getting an ordinary stage 1 map on stage 1, can be configured
  * Fixed "A Moment, Whole" being able to be picked as the starting stage.
  * Fixed "A Moment, Fractured" not being able to be picked as starting stage.
  * Added "Void Locus" to the starting stage blacklist.

* Boss Randomizer:
  * Fixed certain character types not having localized display names

**0.0.1 Changes:**

* First public release
