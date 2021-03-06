package com.hideakin.textsearch.index.repository;

import java.util.Collection;
import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.FileEntity;

@Repository
public interface FileRepository extends JpaRepository<FileEntity,Integer> {

	FileEntity findByFid(int fid);
	List<FileEntity> findAllByGidAndPath(int gid, String path);
	List<FileEntity> findAllByGidAndPathAndStaleFalse(int gid, String path);
	List<FileEntity> findAllByGid(int gid);
	List<FileEntity> findAllByGidAndStaleTrue(int gid);
	List<FileEntity> findAllByGidAndStaleFalse(int gid);
	void deleteByFid(int fid);
	void deleteByFidIn(Collection<Integer> fids);
	void deleteByGid(int gid);

	@Query("SELECT MAX(f.fid) FROM files f")
	Integer getMaxFid();

	@Query("SELECT f FROM files f WHERE f.gid NOT IN (SELECT g.gid FROM file_groups g)")
	List<FileEntity> findUnused();

}
