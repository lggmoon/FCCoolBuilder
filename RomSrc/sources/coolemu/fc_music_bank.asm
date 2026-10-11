;======================================================================
;音乐配置
    .BANK 14
Music_Data_C000
    .ORG $C000
    .incbin "music_c000_1.bin"
    ;;.incbin "Super Contra (J).nsf",$6080,$2000

    .ORG $DF85
    LSR A
    JMP Switch_Music_Bank_A000_A_Pre
    .org $Df89
    .incbin "music_c000_2.bin"


    .BANK 8
    .org $8000
Music_Data_8000
    .incbin "music_8000.bin"
    ;.incbin "Super Contra (J).nsf",$0080,$2000

    .BANK 9 ; ~18
Music_Data_A000
    .org $A000
    .incbin "music_A000_1.bin"
    ;.incbin "Super Contra (J).nsf",$2080,$4000

    .BANK 10
    .org $8000
Music_Data_8001
    .incbin "music_8000.bin"
    ;.incbin "Super Contra (J).nsf",$0080,$2000

    .BANK 11 ; ~18
Music_Data_A002
    .org $A000
    .incbin "music_A000_2.bin"
    ;.incbin "Super Contra (J).nsf",$2080,$4000


