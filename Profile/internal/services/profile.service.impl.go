package services

import (
	"errors"
	"profile/internal/config"
	"profile/internal/models"
)

type ProfileServiceImpl struct {
	cfg     *config.Config
	neoRepo *Neo4jRepo
}

// AddFriend implements ProfileService.
func (p *ProfileServiceImpl) AddFriend(id1 *string, id2 *string) error {
	return (*p.neoRepo).AddFriend(id1, id2)
}

// CreateProfile implements ProfileService.
func (p *ProfileServiceImpl) CreateProfile(profile *models.Profile) error {
	profileExists, err := (*p.neoRepo).GetProfileById(&profile.UserId)

	if err != nil {
		return err
	}
	if profileExists == nil {
		return (*p.neoRepo).CreateProfile(profile)
	}
	return errors.New("profile already exists")
}

// DeleteProfile implements ProfileService.
func (p *ProfileServiceImpl) DeleteProfile(id *string) error {
	return (*p.neoRepo).DeleteProfile(id)
}

// GetProfileById implements ProfileService.
func (p *ProfileServiceImpl) GetProfileById(id *string) (*models.Profile, error) {
	return (*p.neoRepo).GetProfileById(id)
}

// GetProfileByUsername implements ProfileService.
func (p *ProfileServiceImpl) GetProfileByUsername(username *string) (*models.Profile, error) {
	return (*p.neoRepo).GetProfileByUsername(username)
}

// Unfriend implements ProfileService.
func (p *ProfileServiceImpl) Unfriend(id1 *string, id2 *string) error {
	return (*p.neoRepo).Unfriend(id1, id2)
}

func NewProfileService(
	cfg *config.Config,
	neoRepo *Neo4jRepo,
) ProfileService {
	return &ProfileServiceImpl{
		cfg:     cfg,
		neoRepo: neoRepo,
	}
}

func (p *ProfileServiceImpl) Test() string {
	return "Test"
}

func (p *ProfileServiceImpl) CloseDbConnection() {
	(*p.neoRepo).Close()
}
