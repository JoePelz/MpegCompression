ReadImage {
  id = FlvArJ
  t.posx = 0
  t.posy = 0
  name = ReadImage
  path = C:\temp\bmA.tif
}
ColorSpace {
  id = EPbVua
  t.posx = 130
  t.posy = 0
  name = ColorSpace
  inColor = FlvArJ.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = 1w9SeA
  t.posx = 260
  t.posy = 0
  name = To Channels
  inColor = EPbVua.outColor
}
Subsample {
  id = 3PWe9g
  t.posx = 390
  t.posy = 0
  name = Subsample
  inChannels = 1w9SeA.outChannels
  outSamples = 3
  padded = false
}
DCT {
  id = wFbGbo
  t.posx = 520
  t.posy = 0
  name = DCT
  inChannels = 3PWe9g.outChannels
  isInverse = false
  quality = 50
}
DCT {
  id = P5W2Wv
  t.posx = 520
  t.posy = 100
  name = IDCT
  inChannels = wFbGbo.outChannels
  isInverse = true
  quality = 50
}
ReadImage {
  id = 8e9FFG
  t.posx = 0
  t.posy = 200
  name = ReadImage
  path = C:\temp\bmB.tif
}
ColorSpace {
  id = V6MzJJ
  t.posx = 130
  t.posy = 200
  name = ColorSpace
  inColor = 8e9FFG.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = uNRzTM
  t.posx = 260
  t.posy = 200
  name = To Channels
  inColor = V6MzJJ.outColor
}
Subsample {
  id = vbcEEb
  t.posx = 390
  t.posy = 200
  name = Subsample
  inChannels = uNRzTM.outChannels
  outSamples = 3
  padded = true
}
MoVecDecompose {
  id = ixnm6p
  t.posx = 520
  t.posy = 200
  name = Motion Vectors
  radius = 7
  inChannelsPast = P5W2Wv.outChannels
  inChannelsNow = vbcEEb.outChannels
}
DCT {
  id = 4yQDK0
  t.posx = 680
  t.posy = 200
  name = DCT
  inChannels = ixnm6p.outChannels
  isInverse = false
  quality = 50
}
DCT {
  id = 0o9G83
  t.posx = 680
  t.posy = 270
  name = IDCT
  inChannels = 4yQDK0.outChannels
  isInverse = true
  quality = 50
}
MoVecCompose {
  id = KwvHLD
  t.posx = 680
  t.posy = 360
  name = Rebuild Frame
  inChannelsPast = P5W2Wv.outChannels
  inVectors = ixnm6p.outVectors
  inChannels = 0o9G83.outChannels
}
ReadImage {
  id = MPb96v
  t.posx = 0
  t.posy = 500
  name = ReadImage
  path = C:\temp\bmC.tif
}
ColorSpace {
  id = JuQXXH
  t.posx = 130
  t.posy = 500
  name = ColorSpace
  inColor = MPb96v.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = Si0WvL
  t.posx = 260
  t.posy = 500
  name = To Channels
  inColor = JuQXXH.outColor
}
Subsample {
  id = th3Rze
  t.posx = 390
  t.posy = 500
  name = Subsample
  inChannels = Si0WvL.outChannels
  outSamples = 3
  padded = true
}
MoVecDecompose {
  id = DfTYuH
  t.posx = 520
  t.posy = 500
  name = Motion Vectors
  radius = 7
  inChannelsPast = KwvHLD.outChannels
  inChannelsNow = th3Rze.outChannels
}
DCT {
  id = gQcaPg
  t.posx = 680
  t.posy = 500
  name = DCT
  inChannels = DfTYuH.outChannels
  isInverse = false
  quality = 50
}
WriteMulti3Channel {
  id = DurUuQ
  t.posx = 1000
  t.posy = 200
  name = Write Multi
  path = C:\temp\testVid.mdct
  inChannels = wFbGbo.outChannels
  inVectors2 = ixnm6p.outVectors
  inChannels2 = 4yQDK0.outChannels
  inVectors3 = DfTYuH.outVectors
  inChannels3 = gQcaPg.outChannels
}
