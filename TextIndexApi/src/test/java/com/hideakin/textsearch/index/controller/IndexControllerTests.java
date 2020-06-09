package com.hideakin.textsearch.index.controller;

import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.FindTextResponse;
import com.hideakin.textsearch.index.model.PathPositions;
import com.hideakin.textsearch.index.model.UpdateIndexRequest;
import com.hideakin.textsearch.index.model.UpdateIndexResponse;
import com.hideakin.textsearch.index.service.IndexService;

@SpringBootTest
public class IndexControllerTests {

	private MockMvc mockMvc;

	@Mock
	private IndexService indexService;

	@InjectMocks
	private IndexController indexController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(indexController).build();
	}

	@Test
	public void findText_successful() throws Exception {
		FindTextResponse rsp = new FindTextResponse();
		PathPositions[] hits = new PathPositions[] { new PathPositions() };
		hits[0].setPath("quux.h");
		hits[0].setPositions(new int[] { 33 });
		rsp.setHits(hits);
		when(indexService.findText("default", "FOOBAR", SearchOptions.Exact)).thenReturn(rsp);
		mockMvc.perform(MockMvcRequestBuilders.get("/index")
				.param("text", "FOOBAR")
				.param("option", "Exact"))
	    	.andExpect(status().isOk())
	    	.andExpect(jsonPath("$.hits[0].path").value(rsp.getHits()[0].getPath()))
	    	.andExpect(jsonPath("$.hits[0].positions[0]").value(rsp.getHits()[0].getPositions()[0]));
	}

	@Test
	public void findTextByGroup_successful() throws Exception {
		FindTextResponse rsp = new FindTextResponse();
		PathPositions[] hits = new PathPositions[] { new PathPositions() };
		hits[0].setPath("waldo.h");
		hits[0].setPositions(new int[] { 55 });
		rsp.setHits(hits);
		when(indexService.findText("corge", "XYZZY", SearchOptions.Exact)).thenReturn(rsp);
		mockMvc.perform(MockMvcRequestBuilders.get("/index/corge")
				.param("text", "XYZZY")
				.param("option", "Exact"))
	    	.andExpect(status().isOk())
	    	.andExpect(jsonPath("$.hits[0].path").value(rsp.getHits()[0].getPath()))
	    	.andExpect(jsonPath("$.hits[0].positions[0]").value(rsp.getHits()[0].getPositions()[0]));
	}

	@Test
	public void updateIndex_successful() throws Exception {
		UpdateIndexRequest req = new UpdateIndexRequest();
		req.setPath("grault.java");
		req.setTexts(new String[] { "FOO", "BAR", "BAZ" });
		UpdateIndexResponse rsp = new UpdateIndexResponse();
		rsp.setPath(req.getPath());
		rsp.setTexts(req.getTexts());
		when(indexService.updateIndex(eq("default"), argThat(x -> x.getPath().equals(req.getPath())))).thenReturn(rsp);
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(req);
		mockMvc.perform(MockMvcRequestBuilders
				.post("/index")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isOk())
			.andExpect(jsonPath("$.path").value(rsp.getPath()))
			.andExpect(jsonPath("$.texts[0]").value(rsp.getTexts()[0]))
			.andExpect(jsonPath("$.texts[1]").value(rsp.getTexts()[1]))
			.andExpect(jsonPath("$.texts[2]").value(rsp.getTexts()[2]));
	}

	@Test
	public void updateIndexByGroup_successful() throws Exception {
		UpdateIndexRequest req = new UpdateIndexRequest();
		req.setPath("garply.java");
		req.setTexts(new String[] { "FOO", "BAR", "BAZ" });
		UpdateIndexResponse rsp = new UpdateIndexResponse();
		rsp.setPath(req.getPath());
		rsp.setTexts(req.getTexts());
		when(indexService.updateIndex(eq("thud"), argThat(x -> x.getPath().equals(req.getPath())))).thenReturn(rsp);
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(req);
		mockMvc.perform(MockMvcRequestBuilders
				.post("/index/thud")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isOk())
			.andExpect(jsonPath("$.path").value(rsp.getPath()))
			.andExpect(jsonPath("$.texts[0]").value(rsp.getTexts()[0]))
			.andExpect(jsonPath("$.texts[1]").value(rsp.getTexts()[1]))
			.andExpect(jsonPath("$.texts[2]").value(rsp.getTexts()[2]));
	}

	@Test
	public void deleteIndex_successful() throws Exception {
		doNothing().when(indexService).deleteIndex("default");
		mockMvc.perform(MockMvcRequestBuilders.delete("/index"))
			.andExpect(status().isOk());
		verify(indexService, times(1)).deleteIndex("default");
	}

	@Test
	public void deleteIndexByGroup_successful() throws Exception {
		doNothing().when(indexService).deleteIndex("xyzzy");
		mockMvc.perform(MockMvcRequestBuilders.delete("/index/xyzzy"))
			.andExpect(status().isOk());
		verify(indexService, times(1)).deleteIndex("xyzzy");
	}

}
