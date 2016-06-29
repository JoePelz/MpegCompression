ReadImage {
  id = DH2FIR
  t.posx = -150
  t.posy = 0
  name = ReadImage
  path = C:\temp\sunmid.bmp
}
ColorSpace {
  id = 2fEzmr
  t.posx = 10
  t.posy = 0
  name = ColorSpace
  inColor = DH2FIR.outColor
  inSpace = 0
  outSpace = 2
}
ColorToChannels {
  id = kKkeeY
  t.posx = 130
  t.posy = 0
  name = To Channels
  inColor = 2fEzmr.outColor
}
Subsample {
  id = D5ETev
  t.posx = 250
  t.posy = 0
  name = Subsample
  inChannels = kKkeeY.outChannels
  outSamples = 3
  padded = false
}
DCT {
  id = zYLUnj
  t.posx = 370
  t.posy = 0
  name = DCT
  inChannels = D5ETev.outChannels
  isInverse = false
  quality = 50
}
WriteChannels {
  id = ZtEwaV
  t.posx = 490
  t.posy = 0
  name = WriteChannels
  path = C:\temp\testfile.dct
  inChannels = zYLUnj.outChannels
}
ReadChannels {
  id = ZfiOhU
  t.posx = -150
  t.posy = 100
  name = ReadChannels
  path = C:\temp\testfile.dct
}
DCT {
  id = F4D8zf
  t.posx = 10
  t.posy = 100
  name = DCT
  inChannels = ZfiOhU.outChannels
  isInverse = true
  quality = 50
}
ChannelsToColor {
  id = Bx2sit
  t.posx = 130
  t.posy = 100
  name = To Color
  inChannels = F4D8zf.outChannels
}
ColorSpace {
  id = Rkd7Fj
  t.posx = 250
  t.posy = 100
  name = ColorSpace
  inColor = Bx2sit.outColor
  inSpace = 2
  outSpace = 0
}
