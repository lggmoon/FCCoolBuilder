loader:
  lda #$01
  sta CE_MODE

  lda LOADER_OFFSET_L
  sta $5001
  lda LOADER_OFFSET_H
  sta $5002
  lda LOADER_SIZE_L
  sta $5003
  lda LOADER_SIZE_H
  sta $5004
  lda LOADER_IDX_L
  sta $5005
  lda LOADER_IDX_H
  sta $5006

  lda #$CC
  sta $500A

loader_clean_and_start:
  lda #$00
  sta COPY_SOURCE_ADDR
  sta COPY_SOURCE_ADDR+1
  ldy #$02
  ldx #$07
.loop:
  sta [COPY_SOURCE_ADDR], y
  iny
  bne .loop
  inc COPY_SOURCE_ADDR+1
  dex
  bne .loop
.loop2:
  sta [COPY_SOURCE_ADDR], y
  iny
  cpy #LOW(.loop2)
  bne .loop2
  jmp [$FFFC]

; loading tiles to CHR RAM
load_chr:
  jsr enable_chr_write
  lda #$00
  sta PPUADDR
  sta PPUADDR
  ldy #$00
  ldx #$20
.loop:
  lda [COPY_SOURCE_ADDR], y
  sta PPUDATA
  iny
  bne .loop
  inc <COPY_SOURCE_ADDR+1
  dex
  bne .loop
  jsr disable_chr_write
  rts
