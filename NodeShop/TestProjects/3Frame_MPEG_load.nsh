ReadMulti3Channel {
  id = 6Cqcvs
  t.posx = 1000
  t.posy = 0
  name = Read Multi
  path = C:\temp\testVid.mdct
}
DCT {
  id = lJGUpB
  t.posx = 1150
  t.posy = -100
  name = IDCT
  inChannels = 6Cqcvs.outChannels1
  isInverse = true
  quality = 50
}
DCT {
  id = 5QBkrW
  t.posx = 1150
  t.posy = 0
  name = IDCT
  inChannels = 6Cqcvs.outChannels2
  isInverse = true
  quality = 50
}
DCT {
  id = 1rXYx8
  t.posx = 1150
  t.posy = 100
  name = IDCT
  inChannels = 6Cqcvs.outChannels3
  isInverse = true
  quality = 50
}
MoVecCompose {
  id = Ijcagq
  t.posx = 1300
  t.posy = -50
  name = Rebuild Frame
  inChannelsPast = lJGUpB.outChannels
  inVectors = 6Cqcvs.outVectors2
  inChannels = 5QBkrW.outChannels
}
MoVecCompose {
  id = lAwk8T
  t.posx = 1300
  t.posy = 100
  name = Rebuild Frame
  inChannelsPast = Ijcagq.outChannels
  inVectors = 6Cqcvs.outVectors3
  inChannels = 1rXYx8.outChannels
}
ChannelsToColor {
  id = wNbhlm
  t.posx = 1550
  t.posy = -100
  name = To Color
  inChannels = lJGUpB.outChannels
}
ChannelsToColor {
  id = 23VdCx
  t.posx = 1550
  t.posy = 0
  name = To Color
  inChannels = Ijcagq.outChannels
}
ChannelsToColor {
  id = k1ljWH
  t.posx = 1550
  t.posy = 100
  name = To Color
  inChannels = lAwk8T.outChannels
}
ColorSpace {
  id = sYFiJB
  t.posx = 1700
  t.posy = -100
  name = ColorSpace
  inColor = wNbhlm.outColor
  inSpace = 2
  outSpace = 0
}
ColorSpace {
  id = GBdQif
  t.posx = 1700
  t.posy = 0
  name = ColorSpace
  inColor = 23VdCx.outColor
  inSpace = 2
  outSpace = 0
}
ColorSpace {
  id = FKHhnT
  t.posx = 1700
  t.posy = 100
  name = ColorSpace
  inColor = k1ljWH.outColor
  inSpace = 2
  outSpace = 0
}
