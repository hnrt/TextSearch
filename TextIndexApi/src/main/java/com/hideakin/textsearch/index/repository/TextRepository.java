package com.hideakin.textsearch.index.repository;

import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.TextEntity;

@Repository
public interface TextRepository extends JpaRepository<TextEntity,String> {

	public TextEntity findByText(String text);
	public List<TextEntity> findAllByTextStartingWith(String text);
	public List<TextEntity> findAllByTextEndingWith(String text);
	public List<TextEntity> findAllByTextContaining(String text);

}
