ReadImage {
  id = kVahBh
  t.posx = -180
  t.posy = 0
  name = ReadImage
  path = C:\temp\sunmid.bmp
}
ColorSpace {
  id = kB3AaS
  t.posx = -55
  t.posy = 0
  name = ColorSpace
  inColor = kVahBh.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = VsUMyq
  t.posx = 65
  t.posy = 0
  name = To Channels
  inColor = kB3AaS.outColor
}
Subsample {
  id = jNBc56
  t.posx = 200
  t.posy = 0
  name = Subsample
  inChannels = VsUMyq.outChannels
  outSamples = 3
  padded = false
}
DCT {
  id = ibUv0t
  t.posx = 320
  t.posy = 0
  name = DCT
  inChannels = jNBc56.outChannels
  isInverse = false
  quality = 50
}
DCT {
  id = C5hpkW
  t.posx = 440
  t.posy = 0
  name = IDCT
  inChannels = ibUv0t.outChannels
  isInverse = true
  quality = 50
}
ChannelsToColor {
  id = jKxedj
  t.posx = 550
  t.posy = 0
  name = To Color
  inChannels = C5hpkW.outChannels
}
ColorSpace {
  id = jDQgm7
  t.posx = 660
  t.posy = 0
  name = ColorSpace
  inColor = jKxedj.outColor
  inSpace = 2
  outSpace = 0
}
