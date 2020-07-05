package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.model.FileGroupRequest;
import com.hideakin.textsearch.index.model.ObjectDisposition;
import com.hideakin.textsearch.index.service.FileGroupService;

@RestController
public class FileGroupController {

	@Autowired
	private FileGroupService service;

	@RequestMapping(value="/v1/groups",method=RequestMethod.GET)
	public FileGroupInfo[] getGroups() {
		return service.getGroups();
	}

	@RequestMapping(value="/v1/groups/{gid:[0-9]+}",method=RequestMethod.GET)
	public ResponseEntity<?> getGroup(
			@PathVariable int gid) {
		FileGroupInfo body = service.getGroup(gid);
		if (body != null) {
			return new ResponseEntity<>(body, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/groups",method=RequestMethod.POST)
	public ResponseEntity<?> createGroup(
			@RequestBody FileGroupRequest req) {
		ObjectDisposition disp = new ObjectDisposition();
		FileGroupInfo body = service.createGroup(req.getName(), req.getOwnedBy(), disp);
		if (disp.isCreated()) {
			return new ResponseEntity<>(body, HttpStatus.CREATED);
		} else {
			return new ResponseEntity<>(body, HttpStatus.OK);
		}
	}

	@RequestMapping(value="/v1/groups/{gid:[0-9]+}",method=RequestMethod.PUT)
	public ResponseEntity<?> updateGroup(
			@PathVariable int gid,
			@RequestBody FileGroupRequest req) {
		FileGroupInfo body = service.updateGroup(gid, req.getName(), req.getOwnedBy());
		if (body != null) {
			return new ResponseEntity<>(body, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/groups/{gid:[0-9]+}",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteGroup(
			@PathVariable int gid) {
		FileGroupInfo body = service.deleteGroup(gid);
		if (body != null) {
			return new ResponseEntity<>(body, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
