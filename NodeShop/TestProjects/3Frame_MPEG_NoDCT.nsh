ReadImage {
  id = PvQeIk
  t.posx = 0
  t.posy = 0
  name = ReadImage
  path = C:\temp\barbieA.tif
}
ColorSpace {
  id = eUwqjm
  t.posx = 130
  t.posy = 0
  name = ColorSpace
  inColor = PvQeIk.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = adgVDb
  t.posx = 260
  t.posy = 0
  name = To Channels
  inColor = eUwqjm.outColor
}
Subsample {
  id = 3Hz2aH
  t.posx = 390
  t.posy = 0
  name = Subsample
  inChannels = adgVDb.outChannels
  outSamples = 3
  padded = true
}
ReadImage {
  id = tJzHob
  t.posx = 0
  t.posy = 200
  name = ReadImage
  path = C:\temp\barbieB.tif
}
ColorSpace {
  id = WyuC25
  t.posx = 130
  t.posy = 200
  name = ColorSpace
  inColor = tJzHob.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = eBsbvK
  t.posx = 260
  t.posy = 200
  name = To Channels
  inColor = WyuC25.outColor
}
Subsample {
  id = Rb9a3D
  t.posx = 390
  t.posy = 200
  name = Subsample
  inChannels = eBsbvK.outChannels
  outSamples = 3
  padded = true
}
MoVecDecompose {
  id = jgW8lZ
  t.posx = 520
  t.posy = 200
  name = Motion Vectors
  radius = 7
  inChannelsPast = 3Hz2aH.outChannels
  inChannelsNow = Rb9a3D.outChannels
}
MoVecCompose {
  id = rNL8lz
  t.posx = 680
  t.posy = 360
  name = Rebuild Frame
  inChannelsPast = 3Hz2aH.outChannels
  inVectors = jgW8lZ.outVectors
  inChannels = jgW8lZ.outChannels
}
ReadImage {
  id = kNfKKk
  t.posx = 0
  t.posy = 500
  name = ReadImage
  path = C:\temp\barbieC.tif
}
ColorSpace {
  id = sObbq2
  t.posx = 130
  t.posy = 500
  name = ColorSpace
  inColor = kNfKKk.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = jSLeby
  t.posx = 260
  t.posy = 500
  name = To Channels
  inColor = sObbq2.outColor
}
Subsample {
  id = 8IFmY8
  t.posx = 390
  t.posy = 500
  name = Subsample
  inChannels = jSLeby.outChannels
  outSamples = 3
  padded = true
}
MoVecDecompose {
  id = 5NDKLe
  t.posx = 520
  t.posy = 500
  name = Motion Vectors
  radius = 7
  inChannelsPast = rNL8lz.outChannels
  inChannelsNow = 8IFmY8.outChannels
}
WriteMulti3Channel {
  id = jWQtqY
  t.posx = 1000
  t.posy = 200
  name = Write Multi
  path = C:\temp\testVid.mdct
  inChannels = 3Hz2aH.outChannels
  inVectors2 = jgW8lZ.outVectors
  inChannels2 = jgW8lZ.outChannels
  inVectors3 = 5NDKLe.outVectors
  inChannels3 = 5NDKLe.outChannels
}
ReadMulti3Channel {
  id = e7HOhf
  t.posx = 1200
  t.posy = 200
  name = Read Multi
  path = C:\temp\testVid.mdct
}
MoVecCompose {
  id = b7pUPJ
  t.posx = 1400
  t.posy = 200
  name = Rebuild Frame
  inChannelsPast = e7HOhf.outChannels1
  inVectors = e7HOhf.outVectors2
  inChannels = e7HOhf.outChannels2
}
MoVecCompose {
  id = DCmNzS
  t.posx = 1400
  t.posy = 360
  name = Rebuild Frame
  inChannelsPast = b7pUPJ.outChannels
  inVectors = e7HOhf.outVectors3
  inChannels = e7HOhf.outChannels3
}
ChannelsToColor {
  id = kyoXFw
  t.posx = 1600
  t.posy = 100
  name = To Color
  inChannels = e7HOhf.outChannels1
}
ChannelsToColor {
  id = XY2xrK
  t.posx = 1600
  t.posy = 200
  name = To Color
  inChannels = b7pUPJ.outChannels
}
ChannelsToColor {
  id = cpbn0U
  t.posx = 1600
  t.posy = 300
  name = To Color
  inChannels = DCmNzS.outChannels
}
ColorSpace {
  id = hjbZBQ
  t.posx = 1750
  t.posy = 100
  name = ColorSpace
  inColor = kyoXFw.outColor
  inSpace = 2
  outSpace = 0
}
ColorSpace {
  id = Jayt69
  t.posx = 1750
  t.posy = 200
  name = ColorSpace
  inColor = XY2xrK.outColor
  inSpace = 2
  outSpace = 0
}
ColorSpace {
  id = sMWx9s
  t.posx = 1750
  t.posy = 300
  name = ColorSpace
  inColor = cpbn0U.outColor
  inSpace = 2
  outSpace = 0
}
Subsample {
  id = j7rKq9
  t.posx = 1400
  t.posy = 0
  name = Subsample
  inChannels = e7HOhf.outChannels2
  outSamples = 3
  padded = false
}
