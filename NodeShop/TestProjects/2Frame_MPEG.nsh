ReadImage {
  id = P8ecLA
  t.posx = 0
  t.posy = 0
  name = ReadImage
  path = c:\temp\bmA.tif
}
ColorSpace {
  id = vp7NgO
  t.posx = 130
  t.posy = 0
  name = ColorSpace
  inColor = P8ecLA.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = aZyQ2m
  t.posx = 260
  t.posy = 0
  name = To Channels
  inColor = vp7NgO.outColor
}
Subsample {
  id = WG4qTz
  t.posx = 390
  t.posy = 0
  name = Subsample
  inChannels = aZyQ2m.outChannels
  outSamples = 3
  padded = false
}
DCT {
  id = WezBCz
  t.posx = 520
  t.posy = 0
  name = DCT
  inChannels = WG4qTz.outChannels
  isInverse = false
  quality = 50
}
DCT {
  id = 52C6OU
  t.posx = 520
  t.posy = 100
  name = IDCT
  inChannels = WezBCz.outChannels
  isInverse = true
  quality = 50
}
ReadImage {
  id = mfS666
  t.posx = 0
  t.posy = 200
  name = ReadImage
  path = c:\temp\bmB.tif
}
ColorSpace {
  id = Su8wU0
  t.posx = 130
  t.posy = 200
  name = ColorSpace
  inColor = mfS666.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = BNcsyn
  t.posx = 260
  t.posy = 200
  name = To Channels
  inColor = Su8wU0.outColor
}
Subsample {
  id = yYfe6h
  t.posx = 390
  t.posy = 200
  name = Subsample
  inChannels = BNcsyn.outChannels
  outSamples = 3
  padded = true
}
MoVecDecompose {
  id = Ajc8Ie
  t.posx = 520
  t.posy = 200
  name = Motion Vectors
  radius = 7
  inChannelsPast = 52C6OU.outChannels
  inChannelsNow = yYfe6h.outChannels
}
DCT {
  id = BTAUdN
  t.posx = 680
  t.posy = 200
  name = DCT
  inChannels = Ajc8Ie.outChannels
  isInverse = false
  quality = 50
}
WriteMulti2Channel {
  id = JPAxNY
  t.posx = 850
  t.posy = 100
  name = Write Multi
  path = ..\..\testVid.mdct
  inChannels = WezBCz.outChannels
  inVectors2 = Ajc8Ie.outVectors
  inChannels2 = BTAUdN.outChannels
}
ReadMulti2Channel {
  id = rZAy0p
  t.posx = 1000
  t.posy = 100
  name = Read Multi
  path = ..\..\testVid.mdct
}
DCT {
  id = MC3P8p
  t.posx = 1200
  t.posy = 100
  name = DCT
  inChannels = rZAy0p.outChannels1
  isInverse = true
  quality = 50
}
DCT {
  id = p8YlwM
  t.posx = 1200
  t.posy = 200
  name = DCT
  inChannels = rZAy0p.outChannels2
  isInverse = true
  quality = 50
}
MoVecCompose {
  id = TKcUl1
  t.posx = 1350
  t.posy = 200
  name = Rebuild Frame
  inChannelsPast = MC3P8p.outChannels
  inVectors = rZAy0p.outVectors2
  inChannels = p8YlwM.outChannels
}
ChannelsToColor {
  id = phkFYZ
  t.posx = 1500
  t.posy = 100
  name = To Color
  inChannels = MC3P8p.outChannels
}
ChannelsToColor {
  id = d0A6nX
  t.posx = 1500
  t.posy = 200
  name = To Color
  inChannels = TKcUl1.outChannels
}
ColorSpace {
  id = XCjnm5
  t.posx = 1650
  t.posy = 100
  name = ColorSpace
  inColor = phkFYZ.outColor
  inSpace = 2
  outSpace = 0
}
ColorSpace {
  id = amKLeU
  t.posx = 1650
  t.posy = 200
  name = ColorSpace
  inColor = d0A6nX.outColor
  inSpace = 2
  outSpace = 0
}
