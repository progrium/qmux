package mux

import "fmt"

type waiter interface {
	Wait() error
}

// Wait blocks until the session transport has shut down, and returns the
// error causing the shutdown.
func Wait(sess Session) error {
	w, ok := sess.(waiter)
	if !ok {
		return fmt.Errorf("Session does not support waiting")
	}
	return w.Wait()
}
