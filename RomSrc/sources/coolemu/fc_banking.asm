banking_init:
  lda #0
  sta PRG_BANK
  sta CHR_BANK
  sta SRAM_PAGE
  lda #%00000001
  sta CART_CONFIG
  sta CE_MENU_CONFIG
  lda #0
  sta CE_MENU_PRG_BANK
  sta CE_MENU_CHR_BANK
  sta CE_MENU_SRAM_PAGE
  sta CE_MENU_DPCM_BANK
  rts

select_prg_bank:
  sta PRG_BANK
  sta CE_MENU_PRG_BANK
  rts

select_chr_bank:
  sta CHR_BANK
  sta CE_MENU_CHR_BANK
  rts

select_sram_page:
  sta SRAM_PAGE
  sta CE_MENU_SRAM_PAGE
  rts

enable_sram:
  lda CART_CONFIG
  ora #%00000001
  sta CART_CONFIG
  sta CE_MENU_CONFIG
  rts

disable_sram:
  lda CART_CONFIG
  and #%11111110
  sta CART_CONFIG
  sta CE_MENU_CONFIG
  rts

enable_chr_write:
  lda CART_CONFIG
  ora #%00000010
  sta CART_CONFIG
  sta $5007
  rts

disable_chr_write:
  lda CART_CONFIG
  and #%11111101
  sta CART_CONFIG
  sta $5007
  rts

