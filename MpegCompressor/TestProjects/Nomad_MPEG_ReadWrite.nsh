ReadImage {
  id = zm12qA
  t.posx = 0
  t.posy = 0
  name = ReadImage
  path = C:\temp\nomadA.jpg
}
ColorSpace {
  id = EIXVc9
  t.posx = 130
  t.posy = 0
  name = ColorSpace
  inColor = zm12qA.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = jEs329
  t.posx = 260
  t.posy = 0
  name = To Channels
  inColor = EIXVc9.outColor
}
Subsample {
  id = gmvpuW
  t.posx = 390
  t.posy = 0
  name = Subsample
  inChannels = jEs329.outChannels
  outSamples = 3
  padded = false
}
DCT {
  id = n8gRS1
  t.posx = 520
  t.posy = 0
  name = DCT
  inChannels = gmvpuW.outChannels
  isInverse = false
  quality = 50
}
DCT {
  id = PnfZNY
  t.posx = 520
  t.posy = 100
  name = IDCT
  inChannels = n8gRS1.outChannels
  isInverse = true
  quality = 50
}
ReadImage {
  id = 4hXUHi
  t.posx = 0
  t.posy = 200
  name = ReadImage
  path = C:\temp\nomadB.jpg
}
ColorSpace {
  id = 5KHwCN
  t.posx = 130
  t.posy = 200
  name = ColorSpace
  inColor = 4hXUHi.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = TxC5mZ
  t.posx = 260
  t.posy = 200
  name = To Channels
  inColor = 5KHwCN.outColor
}
Subsample {
  id = z9sUFi
  t.posx = 390
  t.posy = 200
  name = Subsample
  inChannels = TxC5mZ.outChannels
  outSamples = 3
  padded = true
}
MoVecDecompose {
  id = TmqW5p
  t.posx = 520
  t.posy = 200
  name = Motion Vectors
  radius = 7
  inChannelsPast = PnfZNY.outChannels
  inChannelsNow = z9sUFi.outChannels
}
DCT {
  id = TArKzL
  t.posx = 680
  t.posy = 200
  name = DCT
  inChannels = TmqW5p.outChannels
  isInverse = false
  quality = 50
}
DCT {
  id = bBVDkZ
  t.posx = 850
  t.posy = 200
  name = IDCT
  inChannels = TArKzL.outChannels
  isInverse = true
  quality = 50
}
MoVecCompose {
  id = FqvIyV
  t.posx = 1000
  t.posy = 200
  name = Rebuild Frame
  inChannelsPast = PnfZNY.outChannels
  inVectors = TmqW5p.outVectors
  inChannels = bBVDkZ.outChannels
}
ChannelsToColor {
  id = pJ06ZD
  t.posx = 1150
  t.posy = 100
  name = To Color
  inChannels = PnfZNY.outChannels
}
ChannelsToColor {
  id = fZRp0V
  t.posx = 1150
  t.posy = 200
  name = To Color
  inChannels = FqvIyV.outChannels
}
ColorSpace {
  id = sZFRT1
  t.posx = 1300
  t.posy = 100
  name = ColorSpace
  inColor = pJ06ZD.outColor
  inSpace = 2
  outSpace = 0
}
ColorSpace {
  id = W8xqUa
  t.posx = 1300
  t.posy = 200
  name = ColorSpace
  inColor = fZRp0V.outColor
  inSpace = 2
  outSpace = 0
}
