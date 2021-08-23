package model

import "strings"

type Cartridge struct {
	Id   string
	Name string
	Size string
}

func (c *Cartridge) ToString() string {
	var sb strings.Builder

	sb.WriteString(c.Id)
	sb.WriteString(": ")
	sb.WriteString(c.Name)
	sb.WriteString(" (")
	sb.WriteString(c.Size)
	sb.WriteString(")")

	return sb.String()

}
