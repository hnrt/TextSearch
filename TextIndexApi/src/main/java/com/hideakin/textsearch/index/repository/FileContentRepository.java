package com.hideakin.textsearch.index.repository;

import java.util.Collection;
import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.FileContentEntity;

@Repository
public interface FileContentRepository extends JpaRepository<FileContentEntity,Integer> {

	FileContentEntity findByFid(int fid);
	void deleteByFid(int fid);
	void deleteByFidIn(Collection<Integer> fids);

	@Query("SELECT c.fid FROM file_contents c WHERE c.fid NOT IN (SELECT f.fid FROM files f)")
	List<Integer> findUnusedFids();

}
