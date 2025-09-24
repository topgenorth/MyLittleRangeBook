open Deedle

let files =
    [| "/home/tom/Dropbox/Firearms/MyLittleRangeBook/ShotView/6x45/6x45_750_gr_Hornad_2025-05-20_11-58-18.csv"
       "/home/tom/Dropbox/Firearms/MyLittleRangeBook/ShotView/6x45/6x45_750_gr_Hornad_2025-05-20_11-33-59.csv"
       "/home/tom/Dropbox/Firearms/MyLittleRangeBook/ShotView/6x45/6x45_750_gr_Hornad_2025-05-11_16-47-35.csv" |]

let f1 = Frame.ReadCsv(files[0])
let f2 = Frame.ReadCsv(files[1])
let f3 = Frame.ReadCsv(files[2])

let f4 = f1.Join f2
f4.SaveCsv("/home/tom/Dropbox/Firearms/MyLittleRangeBook/ShotView/6x45/f4.csv")

let f5= f4.Join f4
f5.SaveCsv("/home/tom/Dropbox/Firearms/MyLittleRangeBook/ShotView/6x45/f5.csv")
