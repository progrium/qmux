package mux

import "fmt"

type waiter interface {
	Wait() error
}

func Wait(sess Session) error {
	w, ok := sess.(waiter)
	if !ok {
		return fmt.Errorf("Session does not support waiting")
	}
	return w.Wait()
}
