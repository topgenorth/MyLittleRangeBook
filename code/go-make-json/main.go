/*
Copyright © 2021 Tom Opgenorth <tom@opgenorth.net>

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
package main

import (
	"opgenorth.net/labradar/commands"
	"opgenorth.net/labradar/pkg/config"
	"os"
)


func main() {
	cfg := config.New()

	cmd := commands.ReadLabradarFileCmd(cfg)
	if err := cmd.Execute(); err != nil {
		os.Exit(1)
	}
}


