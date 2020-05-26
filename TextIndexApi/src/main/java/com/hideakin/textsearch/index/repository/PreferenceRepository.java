package com.hideakin.textsearch.index.repository;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.PreferenceEntity;

@Repository
public interface PreferenceRepository extends JpaRepository<PreferenceEntity,String> {

	PreferenceEntity findByName(String name);

}
