ReadImage {
  id = SYA4dZ
  t.posx = -50
  t.posy = -50
  name = ReadImage
  path = C:\temp\barbieA.tif
}
ColorToChannels {
  id = UYHvbe
  t.posx = 130
  t.posy = -50
  name = To Channels
  inColor = SYA4dZ.outColor
}
ReadImage {
  id = 1Qnhh0
  t.posx = -50
  t.posy = 50
  name = ReadImage
  path = C:\temp\barbieB.tif
}
ColorToChannels {
  id = eqBtEZ
  t.posx = 130
  t.posy = 50
  name = To Channels
  inColor = 1Qnhh0.outColor
}
MoVecDecompose {
  id = WSrfW2
  t.posx = 330
  t.posy = 10
  name = Motion Vectors
  radius = 7
  inChannelsPast = UYHvbe.outChannels
  inChannelsNow = eqBtEZ.outChannels
}
MoVecCompose {
  id = 6Vy5FO
  t.posx = 520
  t.posy = -50
  name = Rebuild Frame
  inChannelsPast = UYHvbe.outChannels
  inVectors = WSrfW2.outVectors
  inChannels = WSrfW2.outChannels
}
Subsample {
  id = boluX7
  t.posx = 330
  t.posy = 150
  name = Subsample
  inChannels = UYHvbe.outChannels
  outSamples = 3
  padded = true
}
Subsample {
  id = o23K48
  t.posx = 330
  t.posy = 250
  name = Subsample
  inChannels = eqBtEZ.outChannels
  outSamples = 3
  padded = true
}
MoVecDecompose {
  id = jZy45V
  t.posx = 500
  t.posy = 250
  name = Motion Vectors
  radius = 7
  inChannelsPast = boluX7.outChannels
  inChannelsNow = o23K48.outChannels
}
MoVecCompose {
  id = eZJbXA
  t.posx = 700
  t.posy = 200
  name = Rebuild Frame
  inChannelsPast = boluX7.outChannels
  inVectors = jZy45V.outVectors
  inChannels = jZy45V.outChannels
}
