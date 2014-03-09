---
title: Player's Guide
layout: content
navbar: true
---

{% include banner.html %}

**Oh shit son!** This page is still under development!
{: .alert .alert-danger}

#RemoteTech 2 Player's Guide

{% include toc.html %}

##Playing RemoteTech

###Antenna Configuration

###The Map View

###The Flight Computer

##Connection Rules

To have a [working connection](../#connections) to the Kerbal Space Center (or a [remote command station](#command-stations)), there must be an unbroken chain of links between satellites and between a satellite and the command center. There is no limit to the number of links in the chain, but *all* links must be valid to establish a connection. An example with three links is shown below.

![A relay sends a transmission from the far side of the Mun towards Kerbin](connectiondemo1.jpg "Mun polar relay"){:.pairedimages} 
![A comsat gets the transmission from the Mun and forwards it to KSC](connectiondemo2.jpg "Kerbin comsat"){:.pairedimages}

**Example:** this probe in low Munar orbit can't link to KSC because the probe is on the far side of the Mun. However, it can link to a relay satellite in polar orbit. The relay also can't link to KSC, because KSC is on the other side of the planet. However, it can link to any of several communications satellites orbiting Kerbin (for clarity, only the best connection is shown). One of these satellites can link to KSC. Therefore, the probe has a working connection with KSC, as relayed by the two intermediate satellites, even though there are nearly 1600 km of solid rock blocking a direct transmission.
{:.caption}


A link will be formed between two satellites if, and only if, three conditions are met. For the purposes of these rules, the KSC is considered a grounded satellite with a [special omnidirectional antenna](#omnidirectional-antennas).

###Line of Sight

The first condition is that there must not be a planet or moon blocking the line of sight between the two satellites. Line of sight calculations assume all planets are perfect spheres, so mountains and other terrain will not block line of sight.

###Range

The second condition is that *both* satellites must have an antenna that can reach as far as the other satellite. A special case is that a link to KSC is impossible unless the satellite establishing a link is within [75,000 km](#omnidirectional-antennas) of Kerbin.

**Example:** a probe with a Communotron 16 antenna (range 2500 km) and a probe with a CommTech-1 dish (350,000,000 km) are located 50,000 km apart. Although the CommTech-1 is one of the most powerful dishes in RemoteTech, the two probes cannot link because the first probe can never link to anything more than 2500 km away. Unless, of course, it has a longer-range dish in addition to the Communotron 16.

###Targeting

The third, and most complex, condition applies only to dish antennas. To establish a link, a dish antenna with sufficient range must be *targeted* at the other satellite, either directly or indirectly. There are three ways to target another satellite:

{::comment}
What's going on here? The kramdown syntax says "The column number of the first non-space character which appears after a definition marker on the same line specifies the indentation that has to be used for the following lines of the definition.", but no style information gets applied!
{:/comment}


Direct Link
: if the dish's target is set to a specific satellite, or to KSC Mission Control, it will maintain a link to that target as long as the line of sight and range conditions are met. A dish in Direct Link mode cannot be used to make connections with anything other than its specific target. Direct link mode is recommended for situations where the other two modes won't work, because keeping direct links up to date can be a lot of work.

Cone
: if the dish's target is set to a planet or moon, it can simultaneously maintain a link to any targets that are within that planet or moon's sphere of influence, *and* that are within a dish-specific angle of the line of sight to that planet. The [list of parts](#dish-antennas) includes the cone angle for each dish, as well as the minimum distance the dish needs to be from Kerbin to see anything to the side of the planet or to see anything in synchronous orbit. Cone mode is recommended for links to relay satellites orbiting another planet, as it will automatically choose the best relay at any given time.

Active Vessel
: if the dish's target is set to "Active Vessel", it will attempt to contact the ship the player is currently flying as if that ship had been selected using Direct Link. The ship will be out of contact whenever the player is *not* directly controlling it. Active Vessel targeting is usually only useful on dedicated communications satellites or [remote command stations](#command-stations), and should only be used to contact isolated deep-space missions where there is no target for a cone and not enough demand for a dedicated direct link. It should **not** be used if the player wants to relay a transmission through a ship other than the one whose antenna is set to Active Vessel.

![A common situation in which active vessel is not appropriate](activerelaybug.png)

**Example:** a mothership is in orbit around a planet and has just detached a lander. Both mothership and lander are equipped with omnidirectional antennas; the mothership also has a powerful dish that is pointed at Kerbin in cone mode. In orbit around Kerbin is a comsat with one of its dishes set to Active Vessel. So long as it's selected, the mothership can connect to mission control on Kerbin, but the lander cannot, because as soon as the player takes control of the lander, the comsat tries to link to *it*, bypassing the mothership entirely. The lander's antenna is too short to link to the comsat, and the mothership can't link because the comsat isn't trying to link to the mothership. To reestablish a connection, the comsat needs to either target the mothership with a **direct link**, or target the planet with a **cone**. Either would create a link between comsat and mothership, letting the mothership act as a relay for the lander.
{:.caption}

##List of Parts

###Probe Cores

All stock probe cores serve as [signal processors](../#signal_processors). In addition, the RC-L01 Remote Guidance Unit can serve as a [command station](../#command_stations), provided a crew of 6 or more kerbals is available to split the jobs of running the ship and sending instructions to nearby probes.

###Omnidirectional Antennas

{::comment}
Yes, the non-breaking spaces are necessary. Without them, when printing the table on a narrow screen, browsers won't be smart enough to realize that notes is the only column that word-wraps well, and will try to create eye-wrenching entries like 2500
km
{:/comment}

Part                | Cost | Mass            | Drag | Range          | Power Drain   | Notes
:-------------------|-----:|:----------------|------|---------------:|:--------------|:------
[Reflectron DP-10](#reflectron-dp-10) | 80   | 0.005&nbsp;tons | 0.2  |    500&nbsp;km | 0.01&nbsp;e/s | Activated on mission start. Not damaged by atmospheric flight
[Communotron 16](#communotron-16) | 150  | 0.005&nbsp;tons | 0.2  |   2500&nbsp;km | 0.13&nbsp;e/s | 
[CommTech EXP-VR-2T](#commtech-exp-vr-2t) | 550  | 0.02&nbsp;tons  | 0.0  |   3000&nbsp;km | 0.18&nbsp;e/s | 
[Communotron 32](#communotron-32) | 150  | 0.01&nbsp;tons  | 0.2  |   5000&nbsp;km | 0.6&nbsp;e/s  | 
KSC Mission Control |      |                 |      | 75,000&nbsp;km |               | Command Station

All science transmissions with stock or RemoteTech antennas cost 7.5 charge per Mit, and they all drain 50 charge per second while transmitting science. This is in addition to the power drain listed in the table, which is for keeping the antenna active and searching for links.

####Reflectron DP-10

The Reflectron DP-10 is a lightweight omnidirectional antenna. Its omnidirectional nature and its ability to function in atmosphere even at high speeds make it an excellent choice for launches and landings, but its short range means it rapidly becomes useless outside low Kerbin orbit. Unlike other antennas, the DP-10 is active by default, although this state can be toggled in the antenna's right-click menu.

![Picture of Reflectron DP-10](antenna_dp10.png)
VAB Category: Science Parts
Tech to Unlock: [Flight Control](http://wiki.kerbalspaceprogram.com/wiki/Flight_Control)
Manufacturer: Parabolic Industries
Cost: 80
Mass: 0.005 tons
Drag: 0.2
Comlink power: 0.01 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 500 km
Reach: Any unbroken line of sight to KSC Mission Control, if below 150 km altitude

**Atmosphere Performance**
Does not break in atmospheric flight.

####Communotron 16

As in the stock game, the Communotron 16 is the starting omnidirectional antenna, essential for transmitting science from those early flights. It also forms the backbown of most player's low-orbit communications networks until the CommTech EXP-VR-2T and Communotron 32 are researched.

![Picture of Communotron 16](antenna_com16.png)
VAB Category: Science Parts
Tech to Unlock: [None](http://wiki.kerbalspaceprogram.com/wiki/Start)
Manufacturer: Ionic Protonic Electronics
Cost: 150
Mass: 0.005 tons
Drag: 0.2
Comlink power: 0.13 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 2500 km
Reach: Low Kerbin Orbit

**Atmosphere Performance**
Maximum ram pressure when deployed: 6 kN/m<sup>2</sup>
Maximum safe speed at sea level: 99 m/s
Maximum safe speed at 10 km: 269 m/s
Minimum safe altitude at 2300 m/s: 32.5 km

####CommTech EXP-VR-2T

The CommTech EXP-VR-2T is an advanced antenna unlocked late in the tech tree. It is mounted on an extendable boom, making it much more compact than the Communotron series when retracted, but slightly larger when deployed. It is slightly more powerful than the Communotron 16.

![Picture of EXP-VR-2T](antenna_expvr2t.png)
VAB Category: Science Parts
Tech to Unlock: [Specialized Electrics](http://wiki.kerbalspaceprogram.com/wiki/Tech_tree#Specialized_Electrics)
Manufacturer: AIES Aerospace
Cost: 150
Mass: 0.005 tons
Drag: 0.2
Comlink power: 0.13 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 3000 km
Reach: Low Kerbin Orbit

**Atmosphere Performance**
Maximum ram pressure when deployed: 6 kN/m<sup>2</sup>
Maximum safe speed at sea level: 99 m/s
Maximum safe speed at 10 km: 269 m/s
Minimum safe altitude at 2300 m/s: 32.5 km

####Communotron 32

The Communotron 32 is the the most powerful omnidirectional antenna available in RemoteTech 2, capable of reaching past kerbosynchonous orbit and filling many moons' spheres of influence. However, it consumes a lot of energy when active.

![Picture of Communotron 32](antenna_com32.png)
VAB Category: Science Parts
Tech to Unlock: [Large Electrics](http://wiki.kerbalspaceprogram.com/wiki/Tech_tree#Large_Electrics)
Manufacturer: Ionic Protonic Electronics
Cost: 150
Mass: 0.01 tons
Drag: 0.2
Comlink power: 0.6 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 5000 km
Reach: Near-Kerbin space, synchronous orbit

**Atmosphere Performance**
Maximum ram pressure when deployed: 3 kN/m<sup>2</sup>
Maximum safe speed at sea level: 70 m/s
Maximum safe speed at 10 km: 190 m/s
Minimum safe altitude at 2300 m/s: 34.9 km

###Dish Antennas

{::comment}
Yes, the non-breaking spaces are necessary. Without them, when printing the table on a narrow screen, browsers won't be smart enough to realize that notes is the only column that word-wraps well, and will try to create eye-wrenching entries like 2500
km
{:/comment}

Antenna           | Cost | Mass            | Drag | Cone Angle | Range          | Power Drain   | Notes
:-----------------|-----:|:----------------|------|:-----------|---------------:|:--------------|:------
[Comms DTS-M1](#comms-dts-m1) | 100  | 0.03&nbsp;tons  | 0.2  | 45&deg;    | 50,000&nbsp;km | 0.82&nbsp;e/s | 
[Reflectron KR-7](#reflectron-kr-7) | 100  | 0.5&nbsp;tons   | 0.2  | 25&deg;    | 90,000&nbsp;km | 0.82&nbsp;e/s | Not damaged by atmospheric flight
[Communotron 88-88](#communotron-88-88) | 900  | 0.025&nbsp;tons | 0.2  | 0.06&deg;  | 40M&nbsp;km    | 0.93&nbsp;e/s | 
[Reflectron KR-14](#reflectron-kr-14) | 100  | 1.0&nbsp;tons   | 0.2  | 0.04&deg;  | 60M&nbsp;km    | 0.93&nbsp;e/s | Not damaged by atmospheric flight
[CommTech-1](#commtech-1) | 800  | 1.0&nbsp;tons   | 0.2  | 0.006&deg; | 350M&nbsp;km   | 2.6&nbsp;e/s  | Not damaged by atmospheric flight
[Reflectron GX-128](#reflectron-gx-128) | 800  | 0.5&nbsp;tons   | 0.2  | 0.005&deg; | 400M&nbsp;km   | 2.8&nbsp;e/s  | 

All science transmissions with stock or RemoteTech antennas cost 7.5 charge per Mit, and they all drain 50 charge per second while transmitting science. This is in addition to the power drain listed in the table, which is for keeping the antenna active and searching for links.

####Comms DTS-M1

The Comms DTS-M1 is the shortest-ranged of the directional dishes. Its wide beam makes it perfect for maintaining contact with multiple satellites within Kerbin's sphere of influence.

![Picture of Comms DTS-M1](antenna_dtsm1.png)
VAB Category: Science Parts
Tech to Unlock: [Science Tech](http://wiki.kerbalspaceprogram.com/wiki/Science_Tech)
Manufacturer: Ionic Symphonic Protonic Electronics
Cost: 100
Mass: 0.03 tons
Drag: 0.2
Comlink power: 0.82 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 50,000 km
Cone Angle: 45&deg;
Cone covers Kerbin at: 1600 km
Cone covers kerbostationary orbit at: 9100 km
Reach: Minmus orbit

**Atmosphere Performance**
Maximum ram pressure when deployed: 6 kN/m<sup>2</sup>
Maximum safe speed at sea level: 99 m/s
Maximum safe speed at 10 km: 269 m/s
Minimum safe altitude at 2300 m/s: 31.5 km

####Reflectron KR-7

The Reflectron KR-7 is the second short-range antenna available from RemoteTech 2. It has a longer range than the Comms DTS-M1, making it well-suited for spacecraft beyond Minmus's orbit. However, its narrow cone reduces its effectiveness at the Mun's distance or closer. The Reflectron KR-7 is too sturdy to be ripped off by atmospheric flight, so if properly targeted it can replace the Reflectron DP-10 as a launch antenna.

![Picture of Reflectron KR-7](antenna_refl7.png)
VAB Category: Science Parts
Tech to Unlock: [Electrics](http://wiki.kerbalspaceprogram.com/wiki/Tech_tree#Electrics)
Manufacturer: Parabolic Industries
Cost: 100
Mass: 0.5 tons
Drag: 0.2
Comlink power: 0.82 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 90,000 km
Cone Angle: 25&deg;
Cone covers Kerbin at: 2800 km
Cone covers kerbostationary orbit at: 16,000 km
Reach: Kerbin sphere of influence

**Atmosphere Performance**
Does not break in atmospheric flight.

####Communotron 88-88

The Communotron 88-88 is by far the lightest interplanetary antenna. It can easily reach all the inner planets, and can even contact Dres when it is on the same side of the sun as Kerbin. However, its narrow cone means that players will have to point it at a specific satellite if they wish to make course corrections while en route to Eve or Duna.

![Picture of Communotron 88-88](antenna_com88-88.png)
VAB Category: Science Parts
Tech to Unlock: [Electronics](http://wiki.kerbalspaceprogram.com/wiki/Electronics)
Manufacturer: Ionic Protonic Electronics
Cost: 1100
Mass: 0.025 tons
Drag: 0.2
Comlink power: 0.93 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 40,000,000 km
Cone Angle: 0.06&deg;
Cone covers Kerbin at: 1,100,000 km
Cone covers kerbostationary orbit at: 6,600,000 km
Reach: Duna (all times), Dres (same side of sun only)

**Atmosphere Performance**
Maximum ram pressure when deployed: 6 kN/m<sup>2</sup>
Maximum safe speed at sea level: 99 m/s
Maximum safe speed at 10 km: 269 m/s
Minimum safe altitude at 2300 m/s: 31.5 km

####Reflectron KR-14

The Reflectron KR-14 is an intermediate-range interplanetary antenna. It can easily reach all the inner planets as well as Dres. Just like the Communotron-88, the KR-14 has a narrow cone and will have difficulty "seeing" communications satellites if pointed directly at Kerbin from too close a range.

![Picture of Reflectron KR-14](antenna_refl14.png)
VAB Category: Science Parts
Tech to Unlock: [Large Electrics](http://wiki.kerbalspaceprogram.com/wiki/Tech_tree#Large_Electrics)
Manufacturer: Parabolic Industries
Cost: 100
Mass: 1.0 tons
Drag: 0.2
Comlink power: 0.93 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 60,000,000 km
Cone Angle: 0.04&deg;
Cone covers Kerbin at: 1,700,000 km
Cone covers kerbostationary orbit at: 9,900,000 km
Reach: Dres (all times), Jool (same side of sun only), Eeloo (periapsis and same side of sun only)

**Atmosphere Performance**
Does not break in atmospheric flight.

####CommTech-1

The CommTech-1 is the first antenna capable of returning signals to Kerbin from the outer solar system. Despite the in-game description, it can reach any planet available in version 0.23 of the game, even Eeloo at apoapsis. However, it has an extremely narrow cone; players should avoid using the dish in cone mode until they pass the orbit of Dres. Even a satellite in orbit around Jool may have occasional connection problems when using cone mode, as it can approach within 52 million km of Kerbin.

![Picture of CommTech-1](antenna_ct1.png)
VAB Category: Science Parts
Tech to Unlock: [Specialized Electrics](http://wiki.kerbalspaceprogram.com/wiki/Tech_tree#Specialized_Electrics)
Manufacturer: AIES Aerospace
Cost: 800
Mass: 1.0 tons
Drag: 0.2
Comlink power: 2.60 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 350,000,000 km
Cone Angle: 0.006&deg;
Cone covers Kerbin at: 11,000,000 km
Cone covers kerbostationary orbit at: 66,000,000 km
Reach: Eeloo (all times)

**Atmosphere Performance**
Does not break in atmospheric flight.

####Reflectron GX-128

The Reflecton-GX-128 is the longest-range antenna available in RemoteTech 2. While it has, for all practical purposes, the same range as the CommTech-1, its foldable construction makes it much lighter.

![Picture of CommTech-1](antenna_ct1.png)
VAB Category: Science Parts
Tech to Unlock: [Advanced Science Tech](http://wiki.kerbalspaceprogram.com/wiki/Tech_tree#Advanced_Science_Tech)
Manufacturer: Parabolic Industries
Cost: 800
Mass: 0.5 tons
Drag: 0.2
Comlink power: 2.80 charge/s
Science power: 50 charge/s
Science efficiency: 7.5 charge/Mit

**Transmission Properties**
Maximum Range: 400,000,000 km
Cone Angle: 0.005&deg;
Cone covers Kerbin at: 14,000,000 km
Cone covers kerbostationary orbit at: 79,000,000 km
Reach: Eeloo (all times)

**Atmosphere Performance**
Maximum ram pressure when deployed: 6 kN/m<sup>2</sup>
Maximum safe speed at sea level: 99 m/s
Maximum safe speed at 10 km: 269 m/s
Minimum safe altitude at 2300 m/s: 31.5 km

##Modding Parts to Work With RemoteTech
